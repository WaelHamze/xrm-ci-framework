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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public class ConfigurationMigrationManager : CommonBase
    {
        #region Variables
        private const string FileLevelRegex = @"^([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12}\.(?:xml|XML))$";

        #endregion

        #region Properties


        #endregion

        #region Constructors

        public ConfigurationMigrationManager(ILogger logger)
            : base(logger)
        {

        }

        #endregion

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

            foreach (FileInfo info in dataInfo.GetFiles())
            {
                info.CopyTo($"{tempDataDirectory}\\{info.Name}");
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

                        Logger.LogInformation($"Processed file: {entityInfo.FullName}");
                    }
                }
            }
            
            using (XmlWriter writer = XmlWriter.Create(tempDataXml))
            {
                entitiesNode.WriteTo(writer);
            }

            foreach (FileInfo entityInfo in tempDataDirectoryInfo.GetFiles("*_data.xml"))
            {
                Logger.LogVerbose($"Deleting file : {entityInfo.FullName}");

                entityInfo.Delete();
            }

            return tempDataDirectory;
        }

        // TODO: Check entity name as directory validity
        public void SplitData(
            string dataFolder,
            CmExpandTypeEnum splitType = CmExpandTypeEnum.Default)
        {
            Logger.LogInformation($"Splitting data xml by type: {Enum.GetName(typeof(CmExpandTypeEnum), splitType)}");
            if (splitType == CmExpandTypeEnum.None)
            {
                return; // No action to be done - safety catch
            }

            if (!Directory.Exists(dataFolder))
            {
                throw new DirectoryNotFoundException($"{dataFolder} not found");
            }

            DirectoryInfo dataInfo = new DirectoryInfo(dataFolder);

            string dataXml = $"{dataFolder}\\data.xml";
            //string schemaXml = $"{dataFolder}\\data_schema.xml";

            if (!File.Exists(dataXml))
            {
                throw new FileNotFoundException($"{dataXml} not found", dataXml);
            }

            FileInfo dataXmlInfo = new FileInfo(dataXml);

            // Has no current functional purpose, can check later on pack/combine if you need it
            //if (!File.Exists(schemaXml))
            //{
            //    throw new FileNotFoundException($"{schemaXml} not found", schemaXml);
            //}

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

                    switch (splitType)
                    {
                        case CmExpandTypeEnum.EntityLevel:
                        case CmExpandTypeEnum.Default:
                        default:
                            string outputFile = $"{dataXmlInfo.Directory.FullName}\\{entityName}_data.xml";

                            using (XmlWriter writer = XmlWriter.Create(outputFile, settings))
                            {
                                entityNode.WriteTo(writer);
                            }
                            Logger.LogInformation($"Processed entity {entityName} to {outputFile}");

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
                            break;
                        case CmExpandTypeEnum.FileLevel:
                            if (!entityNode.HasElements)
                            { // May as well check before creating a directory
                                break;
                            }
                            // Create entity directory if missing
                            string outputEntityDir = Path.Combine(dataInfo.FullName, entityName);
                            if (!Directory.Exists(outputEntityDir))
                            {
                                Directory.CreateDirectory(outputEntityDir);
                            }
                            DirectoryInfo outputEntityDirInfo = new DirectoryInfo(outputEntityDir);
                            // Iterate through records under entity node, create file for each
                            // Could just try entityNode.FirstNode for performance but playing it safe
                            XElement recordsNode = entityNode.Elements().First(e => e.Name == "records");
                            HashSet<string> validRecordSet = new HashSet<string>();
                            foreach (XElement recordNode in recordsNode.Elements())
                            {
                                string recordId = recordNode.Attributes().FirstOrDefault(a => a.Name == "id")?.Value;
                                if (!string.IsNullOrEmpty(recordId) && !validRecordSet.Contains(recordId))
                                {
                                    validRecordSet.Add(recordId);
                                    string outputRecordFile = Path.Combine(outputEntityDir, $"{recordId}.xml");
                                    using (XmlWriter writer = XmlWriter.Create(outputRecordFile, settings))
                                    {
                                        recordNode.WriteTo(writer);
                                    }
                                    Logger.LogVerbose($"Processed entity record {entityName}:{recordId} to {outputRecordFile}");
                                }
                            }
                            // Not safe to delete other directories in the output root, shouldn't be this function's responsibilty
                            // Delete any records not in file list
                            Regex fileLevelRegex = new Regex(FileLevelRegex);
                            FileInfo[] deleteCandidates = outputEntityDirInfo.GetFiles("*.xml");
                            List<FileInfo> filesToDelete = deleteCandidates.Where(fi => fileLevelRegex.IsMatch(fi.Name) && !validRecordSet.Contains(fi.Name.Substring(0, fi.Name.Length-4))).ToList();
                            foreach (FileInfo fileToDelete in filesToDelete)
                            {
                                fileToDelete.Delete();
                            }
                            break;
                    }
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

        #endregion
    }

    //public Component
}
