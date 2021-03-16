using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public class ConfigurationMigrationManager : CommonBase
    {
        #region Constructors

        public ConfigurationMigrationManager(ILogger logger)
            : base(logger)
        {
        }

        #endregion Constructors

        #region Methods

        public void ExpandData(
            string dataZip,
            string folder)
        {
            if (!File.Exists(dataZip))
            {
                throw new FileNotFoundException($"{dataZip} not found", dataZip);
            }

            using (ZipArchive archive = ZipFile.Open(dataZip, ZipArchiveMode.Read))
            {
                DirectoryInfo di = Directory.CreateDirectory(folder);
                string destinationDirectoryFullPath = di.FullName;

                foreach (ZipArchiveEntry file in archive.Entries)
                {
                    string completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, file.FullName));

                    if (!completeFileName.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new IOException("Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");
                    }

                    if (file.Name == "")
                    {// Assuming Empty for Directory
                        Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                        continue;
                    }
                    file.ExtractToFile(completeFileName, true);
                }
            }
        }

        public void CompressData(
            string folder,
            string dataZip)
        {
            if (!Directory.Exists(folder))
            {
                throw new DirectoryNotFoundException($"{folder} not found");
            }

            if (File.Exists(dataZip))
            {
                Logger.LogVerbose($"{dataZip} already exists. Deleting...");

                File.Delete(dataZip);

                Logger.LogVerbose($"{dataZip} Deleted");
            }

            Logger.LogVerbose($"Compressing {folder} to {dataZip}");

            ZipFile.CreateFromDirectory(folder, dataZip);

            Logger.LogInformation($"Compressed {folder} to {dataZip}");
        }

        public void SortDataXml(
            string dataFolder
            )
        {
            if (!Directory.Exists(dataFolder))
            {
                throw new DirectoryNotFoundException($"{dataFolder} not found");
            }

            DirectoryInfo dataInfo = new DirectoryInfo(dataFolder);

            string dataXml = $"{dataFolder}\\data.xml";
            string schemaXml = $"{dataFolder}\\data_schema.xml";

            if (!File.Exists(dataXml))
            {
                throw new FileNotFoundException($"{dataXml} not found", dataXml);
            }

            Logger.LogVerbose($"Loading transform file");

            string currentDir = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.FullName;

            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load($"{currentDir}\\DataXmlTransform.xslt");

            string sortedDataXml = dataXml.Replace(".xml", "_sorted.xml");

            Logger.LogVerbose($"Transforming file into {sortedDataXml}");

            xslt.Transform(dataXml, sortedDataXml);

            File.Copy(sortedDataXml, dataXml, true);

            Logger.LogInformation($"Sorted file {dataXml}");

            File.Delete(sortedDataXml);

            Logger.LogVerbose($"Deleted file {sortedDataXml}");
        }

        public string CombineData(
            string dataFolder)
        {
            if (!Directory.Exists(dataFolder))
            {
                throw new DirectoryNotFoundException($"{dataFolder} not found");
            }

            Logger.LogVerbose($"Combining Data Files from Dir : {dataFolder}");

            DirectoryInfo dataInfo = new DirectoryInfo(dataFolder);

            string tempDataDirectory = $"{Path.GetTempPath()}\\{Guid.NewGuid()}";
            DirectoryInfo tempDataInfo = new DirectoryInfo(dataFolder);

            var tempDataDirectoryInfo = Directory.CreateDirectory(tempDataDirectory);

            Logger.LogVerbose($"Created Temp Dir : {tempDataDirectoryInfo.FullName}");

            foreach (FileInfo info in dataInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                var oldPath = Path.GetDirectoryName(info.FullName);
                var relativePath = oldPath.Remove(0, dataFolder.Length);

                var absolutePath = string.IsNullOrEmpty(relativePath) ? tempDataDirectory : $"{tempDataDirectory}\\{relativePath}";
                if (!Directory.Exists(absolutePath))
                {
                    Directory.CreateDirectory(absolutePath);
                }

                info.CopyTo($"{absolutePath}\\{info.Name}");
            }

            Logger.LogVerbose($"Copied Data Temp Dir : {tempDataDirectoryInfo.FullName}");

            string dataXml = $"{dataFolder}\\data.xml";

            if (!File.Exists(dataXml))
            {
                throw new FileNotFoundException($"{dataXml} not found", dataXml);
            }

            FileInfo dataXmlInfo = new FileInfo(dataXml);

            string tempDataXml = $"{tempDataDirectory}\\data.xml";

            XElement entitiesNode;

            using (var reader = new StreamReader(dataXml))
            {
                entitiesNode = XElement.Load(reader);

                foreach (FileInfo entityInfo in dataInfo.GetFiles("*_data.xml"))
                {
                    using (var entityReader = new StreamReader(entityInfo.FullName))
                    {
                        Logger.LogInformation($"Processing file: {entityInfo.FullName}");

                        XElement entityNode = XElement.Load(entityReader);

                        string entity = (string)entityNode.Attribute("name");

                        var query = from el in entitiesNode.Elements("entity")
                                    where (string)el.Attribute("name") == entity
                                    select el;

                        XElement existingNode = query.FirstOrDefault<XElement>();

                        if (existingNode != null)
                        {
                            Logger.LogVerbose($"Replacing node: {entityNode}");

                            existingNode.ReplaceWith(entityNode);
                        }
                        else
                        {
                            Logger.LogVerbose($"Adding node: {entityNode}");

                            entitiesNode.Add(entityNode);
                        }

                        var entityFolderPath = $"{dataInfo.FullName}\\{entity}";
                        if (Directory.Exists(entityFolderPath))
                        {
                            CombineNode(entityNode, entityFolderPath, "records");
                            CombineNode(entityNode, entityFolderPath, "m2mrelationships");
                        }

                        Logger.LogInformation($"Processed file: {entityInfo.FullName}");
                    }
                }
            }

            using (XmlWriter writer = XmlWriter.Create(tempDataXml))
            {
                entitiesNode.WriteTo(writer);
            }

            foreach (FileInfo entityInfo in tempDataDirectoryInfo.GetFiles("*_data.xml", SearchOption.AllDirectories))
            {
                Logger.LogVerbose($"Deleting file : {entityInfo.FullName}");

                entityInfo.Delete();
            }

            foreach (DirectoryInfo dirInfo in tempDataDirectoryInfo.GetDirectories())
            {
                Logger.LogVerbose($"Deleting directory : {dirInfo.FullName}");

                dirInfo.Delete(true);
            }

            return tempDataDirectory;
        }

        private void CombineNode(XElement entityNode, string path, string nodeName)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo($"{path}\\{nodeName}");
            var query = from el in entityNode.Elements(nodeName)
                        select el;

            XElement node = query.FirstOrDefault();

            foreach (FileInfo fileInfo in directoryInfo.GetFiles("*_data.xml"))
            {
                using (var streamReader = new StreamReader(fileInfo.FullName))
                {
                    XElement childNode = XElement.Load(streamReader);

                    node.Add(childNode);
                }
            }
        }

        public void SplitData(
            string dataFolder, bool splitRecords)
        {
            if (!Directory.Exists(dataFolder))
            {
                throw new DirectoryNotFoundException($"{dataFolder} not found");
            }

            DirectoryInfo dataInfo = new DirectoryInfo(dataFolder);

            string dataXml = $"{dataFolder}\\data.xml";
            string schemaXml = $"{dataFolder}\\data_schema.xml";

            if (!File.Exists(dataXml))
            {
                throw new FileNotFoundException($"{dataXml} not found", dataXml);
            }

            FileInfo dataXmlInfo = new FileInfo(dataXml);

            if (!File.Exists(schemaXml))
            {
                throw new FileNotFoundException($"{schemaXml} not found", schemaXml);
            }

            XElement entitiesNode;
            List<string> entityNames = new List<string>();

            XmlWriterSettings settings = new XmlWriterSettings { Indent = true };

            using (var reader = new StreamReader(dataXml))
            {
                entitiesNode = XElement.Load(reader);

                foreach (XElement entityNode in entitiesNode.Elements())
                {
                    string entityName = entityNode.Attribute("name").Value;

                    Logger.LogInformation($"Processing entity {entityName}");

                    entityNames.Add(entityName);

                    if (splitRecords)
                    {
                        // create entity folder
                        string entityFolderName = $"{dataXmlInfo.Directory.FullName}\\{entityName}";
                        if (Directory.Exists(entityFolderName))
                        {
                            // delete entity folder when it exists to have a clean extract
                            Directory.Delete(entityFolderName, true);
                        }
                        Directory.CreateDirectory(entityFolderName);

                        SplitNode(settings, entityNode, entityFolderName, "records", "id");
                        SplitNode(settings, entityNode, entityFolderName, "m2mrelationships", "sourceid");
                    }

                    string outputFile = $"{dataXmlInfo.Directory.FullName}\\{entityName}_data.xml";

                    using (XmlWriter writer = XmlWriter.Create(outputFile, settings))
                    {
                        entityNode.WriteTo(writer);
                    }

                    Logger.LogInformation($"Processed entity {entityName}");
                }
            }

            foreach (FileInfo entityInfo in dataInfo.GetFiles("*_data.xml"))
            {
                string entity = entityInfo.Name.Substring(0, entityInfo.Name.IndexOf("_data.xml"));

                if (!entityNames.Contains(entity))
                {
                    Logger.LogInformation($"Deleting {entityInfo.Name} as it is not part of data.xml");
                    entityInfo.Delete();
                    Logger.LogInformation($"Deleted {entityInfo.Name}");
                }
            }

            Logger.LogInformation("Removing entity records from data.xml");

            foreach (XElement entityNode in entitiesNode.Elements())
            {
                entityNode.RemoveNodes();
            }

            //entitiesNode.RemoveNodes();

            using (XmlWriter writer = XmlWriter.Create(dataXml, settings))
            {
                entitiesNode.WriteTo(writer);
            }
            Logger.LogInformation("Removed entity nodes from data.xml");
        }

        private void SplitNode(XmlWriterSettings settings, XElement entityNode, string path, string nodeName, string idAttribute)
        {
            foreach (var childNodes in entityNode.Descendants(nodeName))
            {
                string folderName = $"{path}\\{nodeName}";
                Directory.CreateDirectory(folderName);

                foreach (var element in childNodes.Elements())
                {
                    string id = element.Attribute(idAttribute).Value;

                    string recordOutputFile = $"{path}\\{nodeName}\\{id}_data.xml";

                    using (XmlWriter writer = XmlWriter.Create(recordOutputFile, settings))
                    {
                        element.WriteTo(writer);
                    }
                }

                childNodes.RemoveNodes();
            }
        }

        #endregion Methods
    }

    //public Component
}