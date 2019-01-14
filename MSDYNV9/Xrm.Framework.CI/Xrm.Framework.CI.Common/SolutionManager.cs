using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public class SolutionManager : XrmBase
    {
        #region Variables

        private const string ImportSuccess = "success";
        private const string ImportFailure = "failure";
        private const string ImportProcessed = "processed";
        private const string ImportUnprocessed = "Unprocessed";

        #endregion

        #region Properties

        protected IOrganizationService PollingOrganizationService
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public SolutionManager(ILogger logger,
            IOrganizationService organizationService,
            IOrganizationService pollingOrganizationService)
            :base(logger, organizationService)
        {
            PollingOrganizationService = pollingOrganizationService;
        }

        #endregion

        #region Methods

        public SolutionImportResult ImportSolution(
            string solutionFilePath,
            bool publishWorkflows,
            bool convertToManaged,
            bool overwriteUnmanagedCustomizations,
            bool skipProductUpdateDependencies,
            bool holdingSolution,
            bool overrideSameVersion,
            bool importAsync,
            int sleepInterval,
            int asyncWaitTimeout,
            Guid? importJobId,
            bool downloadFormattedLog,
            string logDirectory,
            string logFileName
            )
        {
            Logger.LogInformation("Importing Solution: {0}", solutionFilePath);

            if (!importJobId.HasValue || importJobId.Value == Guid.Empty)
            {
                importJobId = Guid.NewGuid();
            }
            Logger.LogVerbose("ImportJobId {0}", importJobId);

            if (asyncWaitTimeout == 0)
            {
                asyncWaitTimeout = 15 * 60;
            }
            Logger.LogVerbose("AsyncWaitTimeout: {0}", asyncWaitTimeout);

            if (sleepInterval == 0)
            {
                sleepInterval = 15;
            }
            Logger.LogVerbose("SleepInterval: {0}", sleepInterval);

            if (!File.Exists(solutionFilePath))
            {
                Logger.LogError("Solution File does not exist: {0}", solutionFilePath);
                throw new FileNotFoundException("Solution File does not exist", solutionFilePath);
            }

            SolutionImportResult result = null;

            XrmSolutionInfo info = GetSolutionInfo(solutionFilePath);

            if (info == null)
            {
                result = new SolutionImportResult()
                {
                    ErrorMessage = "Invalid Solution File"
                };

                return result;
            }
            else
            {
                Logger.LogInformation("Solution Unique Name: {0}, Version: {1}",
                    info.UniqueName,
                    info.Version);
            }

            bool skipImport = SkipImport(info, holdingSolution, overrideSameVersion);

            if (skipImport)
            {
                Logger.LogInformation("Solution Import Skipped");

                result = new SolutionImportResult()
                {
                    Success = true,
                    ImportSkipped = true
                };

                return result;
            }

            if (downloadFormattedLog)
            {
                if (string.IsNullOrEmpty(logFileName))
                {
                    logFileName = $"{info.UniqueName}_{(info.Version).Replace('.', '_')}_{DateTime.Now.ToString("yyyy_MM_dd__HH_mm")}.xml";
                    Logger.LogVerbose("Settings logFileName to {0}", logFileName);
                }

                if (string.IsNullOrEmpty(logDirectory))
                {
                    logDirectory = Path.GetDirectoryName(solutionFilePath);
                    Logger.LogVerbose("Settings logDirectory to {0}", logDirectory);
                }

                if (!Directory.Exists(logDirectory))
                {
                    Logger.LogError("logDirectory not exist: {0}", logDirectory);
                    throw new DirectoryNotFoundException("logDirectory does not exist");
                }
            }

            byte[] solutionBytes = File.ReadAllBytes(solutionFilePath);

            var importSolutionRequest = new ImportSolutionRequest
            {
                CustomizationFile = solutionBytes,
                PublishWorkflows = publishWorkflows,
                ConvertToManaged = convertToManaged,
                OverwriteUnmanagedCustomizations = overwriteUnmanagedCustomizations,
                SkipProductUpdateDependencies = skipProductUpdateDependencies,
                ImportJobId = importJobId.Value,
                RequestId = importJobId,
                HoldingSolution = holdingSolution
            };

            if (importAsync)
            {
                Logger.LogVerbose(string.Format("Importing solution in Async Mode"));

                var asyncRequest = new ExecuteAsyncRequest
                {
                    Request = importSolutionRequest
                };
                var asyncResponse = OrganizationService.Execute(asyncRequest) as ExecuteAsyncResponse;

                Guid asyncJobId = asyncResponse.AsyncJobId;

                Logger.LogVerbose("Awaiting for Async Operation Completion");

                AsyncUpdateHandler updateHandler = new AsyncUpdateHandler(
                    Logger, PollingOrganizationService, importJobId.Value);

                AsyncOperationManager operationManager
                    = new AsyncOperationManager(Logger, PollingOrganizationService);

                AsyncOperation asyncOperation = operationManager.AwaitCompletion(
                    asyncJobId,
                    asyncWaitTimeout,
                    sleepInterval,
                    updateHandler);

                Logger.LogInformation("Async Operation completed with status: {0}",
                    ((AsyncOperation_StatusCode)asyncOperation.StatusCode.Value).ToString());

                Logger.LogInformation("Async Operation completed with message: {0}",
                    asyncOperation.Message);

                result = VerifySolutionImport(importAsync,
                    importJobId.Value,
                    asyncOperation,
                    null);
            }
            else
            {
                Logger.LogVerbose("Importing solution in Sync Mode");

                SyncImportHandler importHandler = new SyncImportHandler(
                    Logger,
                    OrganizationService,
                    importSolutionRequest);

                ImportJobHandler jobHandler = new ImportJobHandler(
                   Logger,
                   OrganizationService,
                   importHandler);

                Logger.LogVerbose("Creating Import Task");

                Action importAction = () => importHandler.ImportSolution();

                Task importTask = new Task(importAction);

                Logger.LogVerbose("Starting Import Task");

                importTask.Start();

                Logger.LogVerbose("Thread Started. Starting to Query Import Status");

                ImportJobManager jobManager = new ImportJobManager(Logger, PollingOrganizationService);
                jobManager.AwaitImportJob(importJobId.Value, asyncWaitTimeout, sleepInterval, true, jobHandler);

                importTask.Wait();

                result = VerifySolutionImport(importAsync,
                    importJobId.Value,
                    null,
                    importHandler.Error);
            }

            if (result.ImportJobAvailable && downloadFormattedLog)
            {
                ImportJobManager jobManager = new ImportJobManager(Logger, OrganizationService);
                jobManager.SaveFormattedLog(importJobId.Value, logDirectory, logFileName);
            }

            if (result.Success)
            {
                Logger.LogInformation("Solution Import Completed Successfully");
            }
            else
            {
                Logger.LogInformation("Solution Import Failed");
            }

            return result;
        }

        public XrmSolutionInfo GetSolutionInfo(string solutionFilePath)
        {
            Logger.LogVerbose("Reading Solution Zip: {0}", solutionFilePath);

            XrmSolutionInfo info = null;

            try
            {
                string uniqueName;
                string version;

                using (ZipArchive solutionZip = ZipFile.Open(solutionFilePath, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry solutionEntry = solutionZip.GetEntry("solution.xml");

                    using (var reader = new StreamReader(solutionEntry.Open()))
                    {
                        XElement solutionNode = XElement.Load(reader);
                        uniqueName = solutionNode.Descendants("UniqueName").First().Value;
                        version = solutionNode.Descendants("Version").First().Value;
                    }
                }

                info = new XrmSolutionInfo
                {
                    UniqueName = uniqueName,
                    Version = version
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }

            return info;
        }

        public Solution GetSolution(string uniqueName, ColumnSet columns)
        {
            Logger.LogVerbose("Retrieving solution '{0}'", uniqueName);

            QueryByAttribute queryByAttribute = new QueryByAttribute();
            queryByAttribute.EntityName = Solution.EntityLogicalName;
            queryByAttribute.ColumnSet = columns;
            queryByAttribute.Attributes.Add("uniquename");
            queryByAttribute.Values.Add(uniqueName);

            EntityCollection results = OrganizationService.RetrieveMultiple(queryByAttribute);

            if (results.Entities.Count == 0)
            {
                return null;
            }
            else
            {
                return results.Entities[0].ToEntity<Solution>();
            }
        }

        public Solution GetSolution(Guid Id, ColumnSet columns)
        {
            if (columns == null)
            {
                columns = new ColumnSet();
            }

            Solution solution = OrganizationService.Retrieve(Solution.EntityLogicalName,
                Id, columns).ToEntity<Solution>();

            return solution;
        }

        public void DeleteSolution(string uniqueName)
        {
            Solution solution = GetSolution(uniqueName, new ColumnSet());

            if (solution == null)
            {
                Logger.LogWarning("Solution '{0}' was not found", uniqueName);
            }
            else
            {
                Logger.LogVerbose("Deleting solution '{0}'", uniqueName);

                OrganizationService.Delete(Solution.EntityLogicalName,
                    solution.Id);

                Logger.LogInformation("Solution '{0}' was deleted", uniqueName);
            }
        }

        public List<Solution> GetSolutionPatches(string uniqueName)
        {
            Solution parent = GetSolution(uniqueName, new ColumnSet());

            if (parent is null)
            {
                throw new Exception(string.Format("{0} solution can't be found", uniqueName));
            }

            Logger.LogVerbose("Retrieving patches for solution {0}", uniqueName);

            using (var context = new CIContext(OrganizationService))
            {
                var query = from s in context.SolutionSet
                            where s.ParentSolutionId.Id == parent.Id
                            orderby s.Version descending
                            select s;

                List<Solution> solutions = query.ToList<Solution>();

                return solutions;
            }
        }

        public Solution CreatePatch(string uniqueName,
                                    string versionNumber,
                                    string displayName)
        {
            using (var context = new CIContext(OrganizationService))
            {
                if (string.IsNullOrEmpty(versionNumber))
                {
                    Logger.LogVerbose("VersionNumber not supplied. Generating default VersionNumber");

                    var solution = (from sol in context.SolutionSet
                                    where sol.UniqueName == uniqueName || sol.UniqueName.StartsWith(uniqueName + "_Patch")
                                    orderby sol.Version descending
                                    select new Solution { Version = sol.Version, FriendlyName = sol.FriendlyName }).FirstOrDefault();
                    if (solution == null || string.IsNullOrEmpty(solution.Version))
                    {
                        throw new Exception(string.Format("Parent solution with unique name {0} not found.", uniqueName));
                    }

                    string[] versions = solution.Version.Split('.');
                    char dot = '.';
                    versionNumber = string.Concat(versions[0], dot, versions[1], dot, Convert.ToInt32(versions[2]) + 1, dot, 0);
                    Logger.LogVerbose("New VersionNumber: {0}", versionNumber);
                }

                if (string.IsNullOrEmpty(displayName))
                {
                    Logger.LogVerbose("displayName not supplied. Generating default DisplayName");

                    var solution = (from sol in context.SolutionSet
                                    where sol.UniqueName == uniqueName
                                    select new Solution { FriendlyName = sol.FriendlyName }).FirstOrDefault();

                    if (solution == null || string.IsNullOrEmpty(solution.FriendlyName))
                    {
                        throw new Exception(string.Format("Parent solution with unique name {0} not found.", uniqueName));
                    }

                    displayName = solution.FriendlyName;
                }

                var cloneAsPatch = new CloneAsPatchRequest
                {
                    DisplayName = displayName,
                    ParentSolutionUniqueName = uniqueName,
                    VersionNumber = versionNumber,
                };

                CloneAsPatchResponse response = OrganizationService.Execute(cloneAsPatch) as CloneAsPatchResponse;

                Logger.LogInformation("Patch solution created with Id {0}", response.SolutionId);

                Solution patch = GetSolution(response.SolutionId, new ColumnSet(true));

                Logger.LogInformation("Patch solution name: {0}", patch.UniqueName);

                return patch;
            }
        }

        public Solution CloneSolution(string uniqueName,
                                    string versionNumber,
                                    string displayName)
        {
            using (var context = new CIContext(OrganizationService))
            {
                var solution = (from sol in context.SolutionSet
                                where sol.UniqueName == uniqueName
                                select new Solution { Version = sol.Version, FriendlyName = sol.FriendlyName }).FirstOrDefault();
                if (solution == null || string.IsNullOrEmpty(solution.Version))
                {
                    throw new Exception(string.Format("Parent solution with unique name {0} not found.", uniqueName));
                }

                if (string.IsNullOrEmpty(versionNumber))
                {
                    Logger.LogVerbose("VersionNumber not supplied. Generating default VersionNumber");

                    string[] versions = solution.Version.Split('.');
                    char dot = '.';
                    versionNumber = string.Concat(versions[0], dot, Convert.ToInt32(versions[1]) + 1, dot, versions[2], dot, versions[3]);
                    Logger.LogVerbose("New version number {0}", versionNumber);
                }

                if (string.IsNullOrEmpty(displayName))
                {
                    Logger.LogVerbose("displayName not supplied. Generating default displayName");

                    displayName = solution.FriendlyName;
                }

                var cloneAsPatch = new CloneAsSolutionRequest
                {
                    DisplayName = displayName,
                    ParentSolutionUniqueName = uniqueName,
                    VersionNumber = versionNumber,
                };

                CloneAsSolutionResponse response = 
                    OrganizationService.Execute(cloneAsPatch) as CloneAsSolutionResponse;

                Logger.LogInformation("Clone solution created with Id {0}", response.SolutionId);

                Solution clone = GetSolution(response.SolutionId, new ColumnSet(true));

                Logger.LogInformation("Clone solution name: {0}", clone.UniqueName);

                return clone;
            }
        }

        public Solution CreateSolution(string publisherUniqueName,
                                        string uniqueName,
                                        string displayName,
                                        string description,
                                        string versionNumber)
        {
            Logger.LogVerbose("Searching for Publisher: {0}", publisherUniqueName);

            QueryByAttribute queryPublishers = new QueryByAttribute("publisher");
            queryPublishers.Attributes.Add("uniquename");
            queryPublishers.ColumnSet = new ColumnSet(true);
            queryPublishers.Values.Add(publisherUniqueName);

            EntityCollection publishers = OrganizationService.RetrieveMultiple(queryPublishers);

            Logger.LogVerbose("# of Publishers found: {0}", publishers.Entities.Count);

            if (publishers.Entities.Count != 1)
            {
                throw new Exception(string.Format("Unique Publisher with name '{0}' was not found", publisherUniqueName));
            }

            Entity publisher = publishers[0];

            Logger.LogVerbose("Publisher Located Display Name: {0}, Id: {1}", publisher.Attributes["friendlyname"], publisher.Id);

            Logger.LogVerbose("Creating Solution");

            Solution newSolution = new Solution();

            newSolution.UniqueName = uniqueName;
            newSolution.FriendlyName = displayName;
            newSolution.Version = versionNumber;
            newSolution.Description = description;
            newSolution.PublisherId = publisher.ToEntityReference();

            Guid solutionId = OrganizationService.Create(newSolution);

            Logger.LogVerbose("Solution Created with Id: {0}", solutionId);

            return GetSolution(solutionId, new ColumnSet(true));
        }

        private bool SkipImport(
            XrmSolutionInfo info,
            bool holdingSolution,
            bool overrideSameVersion)
        {
            using (var context = new CIContext(OrganizationService))
            {
                ColumnSet columns = new ColumnSet("version");

                if (!holdingSolution)
                {
                    var baseSolution = GetSolution(info.UniqueName, columns);

                    if (baseSolution == null)
                    {
                        Logger.LogInformation("{0} not currently installed.", info.UniqueName);
                    }
                    else
                    {
                        Logger.LogInformation("{0} currently installed with version: {1}", info.UniqueName, info.Version);
                    }

                    if (baseSolution == null ||
                        overrideSameVersion ||
                        (info.Version != baseSolution.Version))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    string upgradeName = $"{info.UniqueName}_Upgrade";

                    var upgradeSolution = GetSolution(upgradeName, columns);

                    if (upgradeSolution == null)
                    {
                        Logger.LogInformation("{0} not currently installed.", upgradeName);
                    }
                    else
                    {
                        Logger.LogInformation("{0} currently installed with version: {1}", upgradeName, info.Version);
                    }

                    if (upgradeSolution == null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }

        private SolutionImportResult VerifySolutionImport(
            bool importAsync,
            Guid importJobId,
            AsyncOperation asyncOperation,
            Exception syncImportException)
        {
            SolutionImportResult result = new SolutionImportResult();

            Logger.LogVerbose("Verifying Solution Import");

            ImportJobManager jobManager = new ImportJobManager(Logger, OrganizationService);

            ImportJob importJob = jobManager.GetImportJob(
                importJobId,
                new ColumnSet("importjobid", "completedon", "progress", "data"));

            if (importJob == null)
            {
                result.ImportJobAvailable = false;
                if (importAsync)
                {
                    result.ErrorMessage = asyncOperation != null ? asyncOperation.Message : "";
                }
                else
                {
                    result.ErrorMessage = syncImportException != null ? syncImportException.Message : "";
                }
                Logger.LogError("Can't verify as import job couldn't be found. Error Message: {0}",
                    result.ErrorMessage);

                return result;
            }
            else
            {
                result.ImportJobAvailable = true;
            }

            if (importJob.Progress == 100)
            {
                Logger.LogInformation("Completed Progress: {0}", importJob.Progress);
            }
            else
            {
                Logger.LogWarning("Completed Progress: {0}", importJob.Progress);
            }
            Logger.LogInformation("Completed On: {0}", importJob.CompletedOn);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(importJob.Data);

            XmlNode resultNode = doc.SelectSingleNode("//solutionManifest/result/@result");
            String solutionImportResult = resultNode != null ? resultNode.Value : null;
            Logger.LogInformation("Import Result: {0}", solutionImportResult);

            XmlNode errorNode = doc.SelectSingleNode("//solutionManifest/result/@errortext");
            String solutionImportError = errorNode != null ? errorNode.Value : null;
            Logger.LogInformation("Import Error: {0}", solutionImportError);

            result.ErrorMessage = solutionImportError;

            XmlNodeList unprocessedNodes = doc.SelectNodes("//*[@processed=\"false\"]");

            result.UnprocessedComponents = unprocessedNodes.Count;

            if (unprocessedNodes.Count > 0)
            {
                Logger.LogWarning("Total number of unprocessed components: {0}", unprocessedNodes.Count);
            }
            else
            {
                Logger.LogInformation("Total number of unprocessed components: {0}", unprocessedNodes.Count);
            }

            if (solutionImportResult == ImportSuccess)
            {
                result.Success = true;
            }

            return result;
        }

        #endregion
    }

    #region Helper Classes

    public class SolutionImportResult
    {
        #region Properties

        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int UnprocessedComponents { get; set; }
        public bool ImportJobAvailable { get; set; }
        public bool ImportSkipped { get; set; }

        #endregion

        #region Constructor

        public SolutionImportResult()
        {
            Success = false;
            ErrorMessage = "";
            UnprocessedComponents = -1;
            ImportJobAvailable = false;
            ImportSkipped = false;
        }

        #endregion
    }

    public class XrmSolutionInfo
    {
        public string UniqueName { get; set; }
        public string Version { get; set; }
    }

    class SyncImportHandler : XrmBase
    {
        #region Properties

        private ImportSolutionRequest ImportRequest
        {
            get;
            set;
        }

        public Exception Error
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public SyncImportHandler(
            ILogger logger,
            IOrganizationService organizationService,
            ImportSolutionRequest importRequest)
            : base(logger, organizationService)
        {
            ImportRequest = importRequest;
        }

        #endregion

        #region Methods

        public void ImportSolution()
        {
            try
            {
                OrganizationService.Execute(ImportRequest);
            }
            catch (Exception ex)
            {
                Error = ex;
            }
        }

        #endregion
    }

    class AsyncUpdateHandler : XrmBase, IAsyncStatusUpdate
    {
        #region Properties

        private Guid ImportJobId
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public AsyncUpdateHandler(
            ILogger logger,
            IOrganizationService organizationService,
            Guid importJobId)
            : base(logger, organizationService)
        {
            ImportJobId = importJobId;
        }

        #endregion

        #region IAsyncStatusUpdate

        public void OnProgressUpdate(AsyncOperation asyncOperation)
        {
            if (asyncOperation.StatusCode.Value == (int)AsyncOperation_StatusCode.InProgress)
            {
                ImportJobManager jobManager
                    = new ImportJobManager(Logger, OrganizationService);

                ImportJob importJob = jobManager.GetImportJob(ImportJobId,
                    new ColumnSet("importjobid", "completedon", "progress"));

                if (importJob != null)
                {
                    Logger.LogVerbose("Import Job Progress: {0}", importJob.Progress);
                }
                else
                {
                    Logger.LogVerbose("Import job not found with Id: {0}", ImportJobId);
                }
            }
        }

        #endregion
    }

    class ImportJobHandler : XrmBase, IJobStatusUpdate
    {
        #region Properties

        private SyncImportHandler ImportHandler
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public ImportJobHandler(
            ILogger logger,
            IOrganizationService organizationService,
            SyncImportHandler importHandler)
            : base(logger, organizationService)
        {
            ImportHandler = importHandler;
        }

        #endregion

        #region IJobStatusUpdate

        public bool OnProgressUpdate(ImportJob importJob)
        {
            Logger.LogVerbose("Checking Sync Import Status");

            if (importJob == null && ImportHandler.Error != null)
            {
                Logger.LogVerbose("Execute Failed and Import Job couldn't be found");
                Logger.LogVerbose("Execute Request Error: {0}", ImportHandler.Error.Message);
                return false;
            }

            return true;
        }

        #endregion
    }

    #endregion
}
