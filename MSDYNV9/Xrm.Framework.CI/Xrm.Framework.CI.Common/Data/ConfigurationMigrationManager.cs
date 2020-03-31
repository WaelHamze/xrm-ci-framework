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
using System.Xml.XPath;
using System.Xml.Xsl;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public class ConfigurationMigrationManager : CommonBase
    {
        #region Variables
        private readonly Regex FileLevelRegex = new Regex(@"^([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12}\.(?:xml|XML))$");
        private readonly XmlWriterSettings CmXmlWriterSettings = new XmlWriterSettings { Indent = true };
        private const long AnnotationContentSizeLimitBytes = 104857600; // 100MB Limit for safety, TODO: Make this user configurable
        private const string AnnotationContentSizeLimitReadable = "100 MB";
        private const string CaseInsensitiveEntityOrRecordLevelAnnotationRecordXPathFormat = "//record[field[@name='isdocument' and @value='True'] and field[@name='filename'] and field[@name='mimetype' and @value='{0}']]";
        private const string CaseSensitiveEntityOrRecordLevelAnnotationRecordXPathFormat = "//record[field[@name='isdocument' and @value='True'] and field[@name='filename' and @value='{0}'] and field[@name='mimetype' and @value='{1}']]";
        private readonly string CaseInsensitiveDataXmlLevelAnnotationRecordXPathFormat = $"//entity[@name='annotation']{CaseInsensitiveEntityOrRecordLevelAnnotationRecordXPathFormat}";
        private readonly string CaseSensitiveDataXmlLevelAnnotationRecordXPathFormat = $"//entity[@name='annotation']{CaseSensitiveEntityOrRecordLevelAnnotationRecordXPathFormat}";
        
        // No XPath 2.0 / case sensitivity support in .NET native at the moment (without other dependencies) - lower-case function
        //private const string ValidCaseInsensitiveAnnotationRecordNodeXPathFormat = "//record[field[@name='isdocument' and @value='True'] and field[@name='filename' and lower-case(@value)='{0}'] and field[@name='mimetype' and @value='{1}']]";
        private const string EntityOrRecordLevelSubstitutionXPathFormat = "//record[field[@name='{0}' and @value='{1}'] and field[@name='{2}']]";
        private readonly string DataXmlLevelSubstitutionXPathFormat = "//entity[@name='{3}']" + EntityOrRecordLevelSubstitutionXPathFormat;

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

            string dataXml = Path.Combine(dataFolder,"data.xml");
            string schemaXml = Path.Combine(dataFolder,"data_schema.xml");

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
            string dataFolder,
            CmExpandTypeEnum combineType = CmExpandTypeEnum.Default)
        {
            if (!Directory.Exists(dataFolder))
            {
                throw new DirectoryNotFoundException($"{dataFolder} not found");
            }

            Logger.LogVerbose($"Combining Data Files from Dir : {dataFolder}");

            DirectoryInfo dataInfo = new DirectoryInfo(dataFolder);

            string tempDataDirectory = Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString());

            string dataXml = Path.Combine(dataFolder,"data.xml");

            if (!File.Exists(dataXml))
            {
                throw new FileNotFoundException($"{dataXml} not found", dataXml);
            }

            FileInfo dataXmlInfo = new FileInfo(dataXml);

            string tempDataXml = Path.Combine(tempDataDirectory,"data.xml");
            DirectoryInfo tempDataDirectoryInfo = null;
            XElement entitiesNode;

            using (var reader = new StreamReader(dataXml))
            {
                entitiesNode = XElement.Load(reader);
            }

            // data.xml record cleanup from SplitData function moved here -> aim is to be non-destructive / better decoupling of methods
            Logger.LogInformation("Removing entity records from data.xml");
            foreach (XElement entityNode in entitiesNode.Elements())
            {
                entityNode.RemoveNodes();
            }
            Logger.LogInformation("Removed entity nodes from data.xml");

            switch (combineType)
            {
                case CmExpandTypeEnum.EntityLevel:
                case CmExpandTypeEnum.Default:
                default:
                    tempDataDirectoryInfo = FileUtilities.DirectoryCopy(dataFolder, tempDataDirectory, Logger);
                    foreach (FileInfo entityInfo in dataInfo.GetFiles("*_data.xml"))
                    {
                        // Better pattern -> try to hold file streams only as long as necessary, can always reacquire later...
                        XElement entityNode;
                        using (var entityReader = new StreamReader(entityInfo.FullName))
                        {
                            Logger.LogInformation($"Processing file: {entityInfo.FullName}");
                            entityNode = XElement.Load(entityReader);
                        }

                        string entity = (string)entityNode.Attribute("name");

                        XElement existingNode = entitiesNode
                                                    .Elements("entity")
                                                    .Where(e => e.Attribute("name").Value == entity)
                                                    .FirstOrDefault();

                        if (existingNode != null)
                        {
                            Logger.LogVerbose($"Replacing entity node: {entity}");
                            existingNode.ReplaceWith(entityNode);
                        }
                        else
                        {
                            Logger.LogVerbose($"Adding node: {entity}");
                            entitiesNode.Add(entityNode);
                        }
                        Logger.LogInformation($"Processed file: {entityInfo.FullName}");
                    }

                    foreach (FileInfo entityInfo in tempDataDirectoryInfo.GetFiles("*_data.xml"))
                    {
                        Logger.LogVerbose($"Deleting file : {entityInfo.FullName}");
                        entityInfo.Delete();
                    }
                    break;
                case CmExpandTypeEnum.RecordLevel:
                    var entityList = new HashSet<string>(entitiesNode.Elements("entity").Select(e => e.Attribute("name").Value));
                    tempDataDirectoryInfo = FileUtilities.DirectoryCopy(dataFolder, tempDataDirectory, Logger, true, entityList);
                    foreach(XElement entityNode in entitiesNode.Elements("entity"))
                    {
                        string entityName = entityNode.Attribute("name").Value;
                        DirectoryInfo targetSubDir = tempDataDirectoryInfo.GetDirectories().FirstOrDefault(di => di.Name == entityName);
                        if (targetSubDir != null && targetSubDir.GetFiles().Count() > 0)
                        {
                            XElement recordsNode = entityNode.Elements("records").FirstOrDefault();
                            if (recordsNode == null)
                            {
                                recordsNode = new XElement("records");
                                entityNode.Add(recordsNode);
                            }
                            foreach (FileInfo entityRecordFileInfo in targetSubDir.GetFiles("*.xml").Where(fi => FileLevelRegex.IsMatch(fi.Name)))
                            {
                                XElement recordNode;
                                using (var recordReader = new StreamReader(entityRecordFileInfo.FullName))
                                {
                                    recordNode = XElement.Load(recordReader);
                                }
                                XElement existingNode = recordsNode.Elements("record").FirstOrDefault(rn => rn.Attribute("id").Value == recordNode.Attribute("id").Value);
                                if (existingNode != null)
                                {
                                    Logger.LogVerbose($"Replacing record: {entityName}:{entityRecordFileInfo.Name}");
                                    existingNode.ReplaceWith(recordNode);
                                }
                                else
                                {
                                    Logger.LogVerbose($"Adding node: {entityName}:{entityRecordFileInfo.Name}");
                                    recordsNode.Add(recordNode);
                                }
                            }
                        }
                        Logger.LogInformation($"Processed entity: {entityName}");
                    }
                    // cleanup copied subdirectories from temp folder
                    foreach (DirectoryInfo directoryToDelete in tempDataDirectoryInfo.GetDirectories().Where(d => entityList.Contains(d.Name)))
                    {
                        directoryToDelete.Delete(recursive:true);
                    }
                    break;
            }

            using (XmlWriter writer = XmlWriter.Create(tempDataXml))
            {
                entitiesNode.WriteTo(writer);
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

            string dataXml = Path.Combine(dataFolder,"data.xml");
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

            using (var reader = new StreamReader(dataXml))
            {
                entitiesNode = XElement.Load(reader);
            }

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
                        string outputFile = Path.Combine(dataXmlInfo.Directory.FullName,$"{entityName}_data.xml");

                        using (XmlWriter writer = XmlWriter.Create(outputFile, CmXmlWriterSettings))
                        {
                            entityNode.WriteTo(writer);
                        }
                        Logger.LogInformation($"Processed entity {entityName} to {outputFile}");

                        
                        break;
                    case CmExpandTypeEnum.RecordLevel:
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
                        XElement recordsNode = entityNode.Elements("records").First();
                        HashSet<string> validRecordSet = new HashSet<string>();
                        foreach (XElement recordNode in recordsNode.Elements())
                        {
                            string recordId = recordNode.Attributes().FirstOrDefault(a => a.Name == "id")?.Value;
                            if (!string.IsNullOrEmpty(recordId) && !validRecordSet.Contains(recordId))
                            {
                                validRecordSet.Add(recordId);
                                string outputRecordFile = Path.Combine(outputEntityDir, $"{recordId}.xml");
                                using (XmlWriter writer = XmlWriter.Create(outputRecordFile, CmXmlWriterSettings))
                                {
                                    recordNode.WriteTo(writer);
                                }
                                Logger.LogVerbose($"Processed entity record {entityName}:{recordId} to {outputRecordFile}");
                            }
                        }
                        // Not safe to delete other directories in the output root, shouldn't be this function's responsibilty
                        // Delete any records not in file list
                        FileInfo[] deleteCandidates = outputEntityDirInfo.GetFiles("*.xml");
                        List<FileInfo> filesToDelete = deleteCandidates.Where(fi => FileLevelRegex.IsMatch(fi.Name) && !validRecordSet.Contains(fi.Name.Substring(0, fi.Name.Length-4))).ToList();
                        foreach (FileInfo fileToDelete in filesToDelete)
                        {
                            fileToDelete.Delete();
                        }
                        break;
                }
            }

            switch (splitType)
            {
                case CmExpandTypeEnum.EntityLevel:
                case CmExpandTypeEnum.Default:
                default:
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
                case CmExpandTypeEnum.RecordLevel: // Cleanup already handled at entity folder level
                    break;
            }

            // Unnecessary byproduct in this method -> Moved it to combinedata method
            //Logger.LogInformation("Removing entity records from data.xml");

            //foreach (XElement entityNode in entitiesNode.Elements())
            //{
            //    entityNode.RemoveNodes();
            //}

            ////entitiesNode.RemoveNodes();
    
            //using (XmlWriter writer = XmlWriter.Create(dataXml, CmXmlWriterSettings))
            //{
            //    entitiesNode.WriteTo(writer);
            //}
            //Logger.LogInformation("Removed entity nodes from data.xml");
        }

        public void MapData(
            string dataFolder, 
            string mappingFile,
            CmExpandTypeEnum expandType = CmExpandTypeEnum.Default,
            bool fileMapCaseSensitive = false)
        {
            // Validate dataFolder existence
            if (!Directory.Exists(dataFolder))
            {
                throw new DirectoryNotFoundException($"{dataFolder} not found");
            }
            DirectoryInfo dataInfo = new DirectoryInfo(dataFolder);
            Logger.LogVerbose($"Mapping Data Files / Substitutions to Dir : {dataFolder}");
            Logger.LogVerbose($"Operating in: {Enum.GetName(typeof(CmExpandTypeEnum), expandType)} mode");

            // Validate mappingFile existence
            FileInfo mappingFileInfo;
            if (!File.Exists(mappingFile))
            {
                throw new FileNotFoundException($"Mapping file not found", mappingFile);
            }
            else
            {
                mappingFileInfo = new FileInfo(mappingFile);
            }
            Logger.LogVerbose($"Using mapping file: {mappingFile}");

            // Enumerate mappingFile / parse
            XElement mapRoot;
            using (var mapReader = new StreamReader(mappingFile))
            {
                mapRoot = XElement.Load(mapReader);
            }
            Logger.LogVerbose("Loaded mapping file");

            List<XElement> annotationMaps = mapRoot.Elements("FileToAnnotation").ToList();
            if (annotationMaps.Count > 0)
            {
                List<XElement> invalidAnnotationMaps = 
                    annotationMaps
                    .Where(m => 
                        m.Attribute("map") == null || string.IsNullOrEmpty(m.Attribute("map").Value) ||
                        m.Attribute("mimetype") == null || string.IsNullOrEmpty(m.Attribute("mimetype").Value))
                    .ToList();
                
                if (invalidAnnotationMaps.Count > 0)
                {
                    Logger.LogWarning($"Found {invalidAnnotationMaps.Count} invalid FileToAnnotation entries, removing...");
                    foreach (XElement invalidAnnotationMap in invalidAnnotationMaps)
                    {
                        annotationMaps.Remove(invalidAnnotationMap);
                    }
                }
                Logger.LogVerbose("Parsed annotation mappings for invalid entries");

                // Group by file path name (eg .\app.js, .\folder1\app.js)
                var duplicateQuery = fileMapCaseSensitive ?
                    annotationMaps.GroupBy(a => Path.GetFileName(a.Attribute("map").Value))
                    : annotationMaps.GroupBy(a => Path.GetFileName(a.Attribute("map").Value.ToLower()));

                List<XElement> duplicateAnnotationMaps = duplicateQuery.Where(g => g.Count() > 1) // For cases where there is a duplicate (eg two 'app.js' maps)
                    .SelectMany(g => g.Where(el => el != g.First())) // Select all but the first in the group
                    .ToList(); 
                if (duplicateAnnotationMaps.Count > 0)
                {
                    Logger.LogWarning($"Found {duplicateAnnotationMaps.Count} duplicate FileToAnnotation entries, excluding...");
                    foreach (XElement duplicateAnnotationMap in duplicateAnnotationMaps)
                    {
                        annotationMaps.Remove(duplicateAnnotationMap);
                    }
                }
                Logger.LogVerbose("Parsed annotation mappings for duplicate entries");
            }
            
            List<XElement> substitutionMaps = mapRoot.Elements("SubstituteRecordValue").ToList();
            if (substitutionMaps.Count > 0)
            {
                List<XElement> invalidSubstitutionMaps =
                    substitutionMaps
                    .Where(m => 
                        m.Attribute("logicalname") == null || string.IsNullOrEmpty(m.Attribute("logicalname").Value) ||
                        m.Attribute("keyattr") == null || string.IsNullOrEmpty(m.Attribute("keyattr").Value) ||
                        m.Attribute("key") == null || string.IsNullOrEmpty(m.Attribute("key").Value) ||
                        m.Attribute("valueattr") == null || string.IsNullOrEmpty(m.Attribute("valueattr").Value) ||
                        m.Attribute("value") == null || string.IsNullOrEmpty(m.Attribute("value").Value))
                    .ToList();

                if (invalidSubstitutionMaps.Count > 0)
                {
                    Logger.LogWarning($"Found {invalidSubstitutionMaps.Count} invalid SubstituteRecordValue entries, removing...");
                    foreach (XElement invalidSubstitutionMap in invalidSubstitutionMaps)
                    {
                        substitutionMaps.Remove(invalidSubstitutionMap);
                    }
                }

                List<XElement> duplicateSubstitutionMaps = 
                    substitutionMaps
                    .GroupBy(a => $"{a.Attribute("logicalname").Value}:{a.Attribute("key").Value}:{a.Attribute("valueattr").Value}") // Group by entity / key value / value attribute
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g.Where(el => el != g.First())) 
                    .ToList();

                if (duplicateSubstitutionMaps.Count > 0)
                {
                    Logger.LogWarning($"Found {duplicateSubstitutionMaps.Count} duplicate SubstituteRecordValue entries, excluding...");
                    foreach (XElement duplicateAnnotationMap in duplicateSubstitutionMaps)
                    {
                        annotationMaps.Remove(duplicateAnnotationMap);
                    }
                }
            }
            
            switch (expandType)
            {
                case CmExpandTypeEnum.EntityLevel:
                case CmExpandTypeEnum.Default:
                default:
                    if (annotationMaps.Count > 0)
                    {
                        // Find annotation xml in folder / load it
                        string annotationFilePath = Path.Combine(dataFolder, "annotation_data.xml");
                        if (!File.Exists(annotationFilePath))
                        {
                            throw new FileNotFoundException($"Entity-level annotation file not found", annotationFilePath);
                        }
                        XElement annotationRoot;
                        using (var annotationReader = new StreamReader(annotationFilePath))
                        {
                            annotationRoot = XElement.Load(annotationReader);
                        }
                        Logger.LogVerbose("Loaded entity-level annotation file");
                        bool updateAnnotationFile = false;

                        foreach (XElement annotationMap in annotationMaps)
                        {
                            // Validate source file existence
                            string sourceFilePath = Path.Combine(dataInfo.FullName,annotationMap.Attribute("map").Value);
                            if (!File.Exists(sourceFilePath))
                            {
                                Logger.LogWarning($"Target annotation map source: {sourceFilePath} not found, skipping...");
                                continue;
                            }
                            else
                            {
                                FileInfo sourceFileInfo = new FileInfo(sourceFilePath);
                                if (sourceFileInfo.Length > AnnotationContentSizeLimitBytes)
                                {
                                    Logger.LogWarning($"Target annotation map source: {sourceFilePath} exceeds the transform file size limit of {AnnotationContentSizeLimitReadable}");
                                    continue;
                                }
                            }
                            
                            // Validate node existence in annotation XML
                            string targetFileName = Path.GetFileName(annotationMap.Attribute("map").Value);
                            if (!fileMapCaseSensitive)
                            {
                                targetFileName = targetFileName.ToLower();
                            }
                            string targetMimeType = annotationMap.Attribute("mimetype").Value;

                            XElement targetNode;
                            if (fileMapCaseSensitive)
                            {
                                targetNode = annotationRoot
                                                .XPathSelectElements(string.Format(CaseSensitiveEntityOrRecordLevelAnnotationRecordXPathFormat, targetFileName, targetMimeType))
                                                .FirstOrDefault();
                            }
                            else
                            {
                                // Had to write this due to lack of lower-case XPath2.0 support in native System.Xml.XPath
                                targetNode = annotationRoot
                                                .XPathSelectElements(string.Format(CaseInsensitiveEntityOrRecordLevelAnnotationRecordXPathFormat, targetMimeType))
                                                .FirstOrDefault(r => r.Elements("field")
                                                                        .Any(f => f.Attribute("name") != null && f.Attribute("name").Value == "filename" &&
                                                                                    f.Attribute("value") != null && f.Attribute("value").Value.ToLower() == targetFileName
                                                                            )
                                                                );
                            }
                            if (targetNode == null)
                            {
                                Logger.LogWarning($"Target annotation node for map: {targetFileName} not found in entity level XML, skipping...");
                                continue;
                            }

                            // Load replacement content/encode, if you run out of RAM it's your own fault!

                            byte[] contentBytes = File.ReadAllBytes(sourceFilePath);
                            string encodedContent = System.Convert.ToBase64String(contentBytes);
                            targetNode.XPathSelectElements("./field[@name='documentbody']")
                                        .First()
                                        .SetAttributeValue("value", encodedContent);

                            if (!updateAnnotationFile)
                            {
                                updateAnnotationFile = true;
                            }

                            Logger.LogInformation($"Updated annotation documentbody for map: {targetFileName}");
                        }

                        // Only update the file if we need to
                        if (updateAnnotationFile)
                        {
                            using (XmlWriter writer = XmlWriter.Create(annotationFilePath, CmXmlWriterSettings))
                            {
                                annotationRoot.WriteTo(writer);
                            }
                        }                       
                    }
                    if (substitutionMaps.Count > 0)
                    {
                        List<string> targetEntities = substitutionMaps
                                                .Select(m => m.Attribute("logicalname").Value)
                                                .Distinct()
                                                .ToList();
                        foreach (string targetEntity in targetEntities)
                        {
                            // Locate file
                            string targetFilePath = Path.Combine(dataFolder, $"{targetEntity}_data.xml");
                            bool updateTargetFile = false;
                            if (!File.Exists(targetFilePath))
                            {
                                Logger.LogWarning($"Cannot locate target entity level file: {targetFilePath} for SubstituteRecordValue entity target, skipping...");
                                continue;
                            }
                            XElement entityRoot;
                            using (var entityReader = new StreamReader(targetFilePath))
                            {
                                entityRoot = XElement.Load(targetFilePath);
                            }
                            
                            foreach (XElement substitutionMap in substitutionMaps.Where(m => m.Attribute("logicalname").Value == targetEntity))
                            {
                                string keyAttr = substitutionMap.Attribute("keyattr").Value;
                                string key = substitutionMap.Attribute("key").Value;
                                string valueAttr = substitutionMap.Attribute("valueattr").Value;
                                XElement targetNode = entityRoot
                                                        .XPathSelectElements(string.Format(EntityOrRecordLevelSubstitutionXPathFormat, keyAttr, key, valueAttr))
                                                        .FirstOrDefault();
                                if (targetNode != null)
                                {
                                    string value = substitutionMap.Attribute("value").Value;
                                    targetNode.XPathSelectElements($"//field[@name='{valueAttr}']")
                                                .First()
                                                .SetAttributeValue("value", value);
                                    updateTargetFile = true;
                                    Logger.LogInformation($"Substituted value for SubstituteRecordValue target: {key}");
                                }
                                else
                                {
                                    Logger.LogWarning($"Did not find matching record to substitude for SubstituteRecordValue target: {key}, skipping...");
                                    continue;
                                }
                            }
                            if (updateTargetFile)
                            {
                                using (XmlWriter writer = XmlWriter.Create(targetFilePath, CmXmlWriterSettings))
                                {
                                    entityRoot.WriteTo(writer);
                                }
                                Logger.LogInformation($"Updated entity level file for entity: {targetEntity}");
                            }
                        }
                        
                    }
                    break;
                case CmExpandTypeEnum.RecordLevel:
                    if (annotationMaps.Count > 0)
                    {
                        // Find annotation folder
                        string annotationFolderPath = Path.Combine(dataFolder, "annotation");
                        if (!Directory.Exists(annotationFolderPath))
                        {
                            throw new DirectoryNotFoundException($"Record-level annotation folder: {annotationFolderPath} not found");
                        }
                        DirectoryInfo annotationFolderInfo = new DirectoryInfo(annotationFolderPath);

                        // Check if we can't find any replacement files / check their size to save us some time later
                        List<XElement> annotationMapsNotFound = annotationMaps
                                                                    .Where(m => !File.Exists(Path.Combine(dataInfo.FullName, m.Attribute("map").Value)))
                                                                    .ToList();
                        List<XElement> annotationMapsTooLarge = annotationMaps
                                                                    .Except(annotationMapsNotFound)
                                                                    .Where(m => new FileInfo(Path.Combine(dataInfo.FullName, m.Attribute("map").Value)).Length > AnnotationContentSizeLimitBytes)
                                                                    .ToList();
                        if (annotationMapsNotFound.Count > 0)
                        {
                            Logger.LogWarning($"Could not find {annotationMapsNotFound.Count} FileToAnnotation map sources, skipping them...");
                            foreach (XElement mapToDelete in annotationMapsNotFound)
                            {
                                annotationMaps.Remove(mapToDelete);
                            }
                        }

                        if (annotationMapsTooLarge.Count > 0)
                        {
                            Logger.LogWarning($"Found {annotationMapsTooLarge.Count} FileToAnnotation map sources that exceed the file size limit of {AnnotationContentSizeLimitReadable}, skipping them...");
                            foreach (XElement mapToDelete in annotationMapsTooLarge)
                            {
                                annotationMaps.Remove(mapToDelete);
                            }
                        }

                        if (annotationMaps.Count > 0)
                        {
                            // <Filename<MimeType,FilePath>>
                            Dictionary<string, Tuple<string, string>> targetAttachmentInfo = annotationMaps
                                                                                    .ToDictionary(
                                                                                        m => fileMapCaseSensitive ? 
                                                                                            Path.GetFileName(m.Attribute("map").Value) 
                                                                                            : Path.GetFileName(m.Attribute("map").Value.ToLower()), 
                                                                                        m => new Tuple<string, string>(
                                                                                            m.Attribute("mimetype").Value, 
                                                                                            Path.Combine(dataInfo.FullName, m.Attribute("map").Value)
                                                                                        )
                                                                                    );
                            
                            // Enumerate through the valid records in the folder, look for matches (reverse of entity-level approach)
                            foreach (FileInfo annotationRecordFileInfo in annotationFolderInfo
                                                                            .GetFiles("*.xml")
                                                                            .Where(fi => FileLevelRegex.IsMatch(fi.Name)))
                            {
                                if (targetAttachmentInfo.Count == 0)
                                {
                                    Logger.LogVerbose("All annotation targets have been matched, continuing to record value substitutions...");
                                    break;
                                }

                                XElement annotationRecordRoot;
                                using (var annotationFileReader = new StreamReader(annotationRecordFileInfo.FullName))
                                {
                                    annotationRecordRoot = XElement.Load(annotationFileReader);
                                }

                                string recordFilename = annotationRecordRoot
                                                                .XPathSelectElements("//field[@name='filename']")
                                                                .FirstOrDefault()
                                                                ?.Attribute("value")
                                                                ?.Value;
                                if (!fileMapCaseSensitive)
                                {
                                    recordFilename = recordFilename.ToLower();
                                }
                                string recordMimeType = annotationRecordRoot
                                                            .XPathSelectElements("//field[@name='mimetype']")
                                                            .FirstOrDefault()
                                                            ?.Attribute("value")
                                                            ?.Value;
                                // If the record's filename/mimetype match something in our annotation substitution list
                                if (!string.IsNullOrEmpty(recordFilename) && !string.IsNullOrEmpty(recordMimeType) &&
                                    targetAttachmentInfo.Keys.Contains(recordFilename) && targetAttachmentInfo[recordFilename].Item1 == recordMimeType)
                                {
                                    string newContentFilePath = targetAttachmentInfo[recordFilename].Item2;
                                    byte[] contentBytes = File.ReadAllBytes(newContentFilePath);
                                    string encodedContent = System.Convert.ToBase64String(contentBytes);
                                    annotationRecordRoot
                                        .XPathSelectElements("//field[@name='documentbody']")
                                        .First()
                                        .SetAttributeValue("value", encodedContent);

                                    using (XmlWriter writer = XmlWriter.Create(annotationRecordFileInfo.FullName, CmXmlWriterSettings))
                                    {
                                        annotationRecordRoot.WriteTo(writer);
                                    }
                                    Logger.LogInformation($"Updated annotation documentbody for map: {recordFilename}");
                                    targetAttachmentInfo.Remove(recordFilename); // Remove it, so we don't have to check for it again
                                }
                            }
                        }
                        else
                        {
                            Logger.LogVerbose("No valid FileToAnnotation maps to process, continuing...");
                        }
                    }
                    else
                    {
                        Logger.LogVerbose("No valid FileToAnnotation maps to process, continuing...");
                    }

                    if (substitutionMaps.Count > 0)
                    {
                        Logger.LogVerbose($"Attempting value substitution for {substitutionMaps.Count} SubstituteRecordValue entries");
                        List<string> targetEntities = substitutionMaps
                                                .Select(m => m.Attribute("logicalname").Value)
                                                .Distinct()
                                                .ToList();
                        foreach (string targetEntity in targetEntities)
                        {
                            // Find target entity directory
                            string entityFolderPath = Path.Combine(dataFolder, targetEntity);
                            DirectoryInfo entityFolderInfo;
                            if (!Directory.Exists(entityFolderPath))
                            {
                                Logger.LogWarning($"No expanded entity folder found for target SubstituteRecordValue entity with logical name: {targetEntity}, skipping...");
                                continue;
                            }
                            else
                            {
                                entityFolderInfo = new DirectoryInfo(entityFolderPath);
                            }
                            // <KeyAttr,KeyValue,IList<Tuple<ValueAttr,ValueValue>> --> Group all value updates on a field for a given key/key attr combo for the current entity
                            // I hated writing this, but it's the most efficient memory map and if converted to strongly typed classes they'd never be used elsewhere...
                            var entitySubstitutions = substitutionMaps
                                                        .Where(m => m.Attribute("logicalname").Value == targetEntity)
                                                        .Select(m => new Tuple<string,string,List<Tuple<string,string>>>(
                                                                m.Attribute("keyattr").Value, 
                                                                m.Attribute("key").Value,
                                                                substitutionMaps
                                                                    .Where(m2 =>    m2.Attribute("key").Value == m.Attribute("key").Value && 
                                                                                    m2.Attribute("keyattr").Value == m.Attribute("keyattr").Value)
                                                                    .Select(m3 => new Tuple<string, string>(m3.Attribute("valueattr").Value, m3.Attribute("value").Value))
                                                                    .ToList()
                                                            )
                                                        )
                                                        .ToList();

                            Logger.LogVerbose("Enumerating record XML files for substitution matches");
                            foreach (FileInfo entityRecordFileInfo in entityFolderInfo
                                                                            .GetFiles("*.xml")
                                                                            .Where(fi => FileLevelRegex.IsMatch(fi.Name)))
                            {
                                if (entitySubstitutions.Count == 0)
                                {
                                    Logger.LogVerbose($"No further SubstituteRecordValue maps to be substituted, ending record XML scan...");
                                    break;
                                }
                                bool updateRecordFile = false;
                                // Enumerate through all entity record XMLs in the folder, looking for matches
                                XElement entityRecordRoot;
                                using (var recordFileReader = new StreamReader(entityRecordFileInfo.FullName))
                                {
                                    entityRecordRoot = XElement.Load(recordFileReader);
                                }
                                // Have to evaluate current record against entire substitution key/keyattr set -> LINQ required over dynamic XPATH
                                XElement matchedSubsitutionRecordField = entityRecordRoot
                                                                            .Elements()
                                                                            .Where(f => entitySubstitutions.Any(sub => sub.Item1 == f.Attribute("name").Value && sub.Item2 == f.Attribute("value").Value)) // Record has a field attribute matching a target key attr/value
                                                                            .FirstOrDefault();
                                if (matchedSubsitutionRecordField != null)
                                { // Map all subsitution values for this record
                                    var matchedKeyAttr = matchedSubsitutionRecordField.Attribute("name").Value;
                                    var matchedKeyValue = matchedSubsitutionRecordField.Attribute("value").Value;
                                    var substitutionMap = entitySubstitutions
                                                                        .Where(m => m.Item1 == matchedKeyAttr && m.Item2 == matchedKeyValue)
                                                                        .First();
                                    int updateCount = 0;
                                    foreach (var substitution in substitutionMap.Item3)
                                    { // Make sure it exists
                                        var entityRecordField = entityRecordRoot.XPathSelectElements($"//field[@name='{substitution.Item1}']").FirstOrDefault();
                                        if (entityRecordField != null)
                                        {
                                            entityRecordField.SetAttributeValue("value", substitution.Item2);
                                            updateRecordFile = true;
                                            updateCount++;
                                        }
                                    }

                                    if (updateRecordFile)
                                    {
                                        Logger.LogVerbose($"{updateCount} substitution value matches found for record file: {entityRecordFileInfo.Name}, updating file...");
                                        using (XmlWriter writer = XmlWriter.Create(entityRecordFileInfo.FullName, CmXmlWriterSettings))
                                        {
                                            entityRecordRoot.WriteTo(writer);
                                        }
                                    }

                                    // Remove the found map from the list so we don't keep checking all records once done
                                    entitySubstitutions.Remove(substitutionMap);
                                }
                            }
                        }
                    }
                    else
                    {
                        Logger.LogVerbose("No valid SubstituteRecordValue maps to process, continuing...");
                    }

                    break;
                case CmExpandTypeEnum.None: // Operate on a complete data.xml file (assume not compressed)
                    // TODO: Code for this is basically the same as entity-level/default, need to factor out commonalities
                    string dataXmlPath = Path.Combine(dataFolder, "data.xml");
                    FileInfo dataXmlFileInfo;
                    if (!File.Exists(dataXmlPath))
                    {
                        throw new FileNotFoundException($"Target data.xml file not found", dataXmlPath);
                    }
                    else
                    {
                        dataXmlFileInfo = new FileInfo(dataXmlPath);
                    }
                    bool updateDataXmlFile = false;
                    if (annotationMaps.Count > 0 || substitutionMaps.Count > 0)
                    {
                        XElement dataRoot;
                        using (var dataFileReader = new StreamReader(dataXmlFileInfo.FullName))
                        {
                            dataRoot = XElement.Load(dataFileReader);
                        }

                        foreach (XElement annotationMap in annotationMaps)
                        {
                            // Validate source file existence
                            string sourceFilePath = Path.Combine(dataInfo.FullName, annotationMap.Attribute("map").Value);
                            if (!File.Exists(sourceFilePath))
                            {
                                Logger.LogWarning($"Target annotation map source: {sourceFilePath} not found, skipping...");
                                continue;
                            }
                            else
                            {
                                FileInfo sourceFileInfo = new FileInfo(sourceFilePath);
                                if (sourceFileInfo.Length > AnnotationContentSizeLimitBytes)
                                {
                                    Logger.LogWarning($"Target annotation map source: {sourceFilePath} exceeds the transform file size limit of {AnnotationContentSizeLimitReadable}");
                                    continue;
                                }
                            }

                            // Validate node existence in annotation XML
                            string targetFileName = Path.GetFileName(annotationMap.Attribute("map").Value);
                            if (!fileMapCaseSensitive)
                            {
                                targetFileName = targetFileName.ToLower();
                            }
                            string targetMimeType = annotationMap.Attribute("mimetype").Value;

                            XElement targetNode;
                            if (fileMapCaseSensitive)
                            {
                                targetNode = dataRoot
                                                .XPathSelectElements(string.Format(CaseSensitiveDataXmlLevelAnnotationRecordXPathFormat, targetFileName, targetMimeType))
                                                .FirstOrDefault();
                            }
                            else
                            {
                                // Had to write this due to lack of lower-case XPath2.0 support in native System.Xml.XPath
                                targetNode = dataRoot
                                                .XPathSelectElements(string.Format(CaseInsensitiveDataXmlLevelAnnotationRecordXPathFormat, targetMimeType))
                                                .FirstOrDefault(r => r.Elements("field")
                                                                        .Any(f => f.Attribute("name") != null && f.Attribute("name").Value == "filename" &&
                                                                                    f.Attribute("value") != null && f.Attribute("value").Value.ToLower() == targetFileName
                                                                            )
                                                                );
                            }
                            if (targetNode == null)
                            {
                                Logger.LogWarning($"Target annotation node for map: {targetFileName} not found in data XML, skipping...");
                                continue;
                            }

                            // Load replacement content/encode, if you run out of RAM it's your own fault!

                            byte[] contentBytes = File.ReadAllBytes(sourceFilePath);
                            string encodedContent = System.Convert.ToBase64String(contentBytes);
                            targetNode.XPathSelectElements("./field[@name='documentbody']")
                                        .First()
                                        .SetAttributeValue("value", encodedContent);

                            updateDataXmlFile = true;

                            Logger.LogInformation($"Updated annotation documentbody for map: {targetFileName}");
                        }

                        foreach (XElement substitutionMap in substitutionMaps)
                        {
                            string keyAttr = substitutionMap.Attribute("keyattr").Value;
                            string key = substitutionMap.Attribute("key").Value;
                            string valueAttr = substitutionMap.Attribute("valueattr").Value;
                            string entityLogicalName = substitutionMap.Attribute("logicalname").Value;
                            XElement targetNode = dataRoot
                                                    .XPathSelectElements(string.Format(DataXmlLevelSubstitutionXPathFormat, keyAttr, key, valueAttr, entityLogicalName))
                                                    .FirstOrDefault();
                            if (targetNode != null)
                            {
                                string value = substitutionMap.Attribute("value").Value;
                                targetNode.XPathSelectElements($"//field[@name='{valueAttr}']")
                                            .First()
                                            .SetAttributeValue("value", value);
                                updateDataXmlFile = true;
                                Logger.LogInformation($"Substituted value for SubstituteRecordValue target: {key}");
                            }
                            else
                            {
                                Logger.LogWarning($"Did not find matching record to substitute for SubstituteRecordValue target: {key}, skipping...");
                                continue;
                            }
                        }

                        if (updateDataXmlFile)
                        {
                            using (XmlWriter writer = XmlWriter.Create(dataXmlPath, CmXmlWriterSettings))
                            {
                                dataRoot.WriteTo(writer);
                            }
                            Logger.LogInformation($"Updated data.xml file");
                        }
                    }
                    
                    break;
            }
        }
        #endregion
    }

    //public Component
}
