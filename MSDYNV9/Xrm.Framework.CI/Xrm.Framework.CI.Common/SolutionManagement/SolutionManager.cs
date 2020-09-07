using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            Logger.LogInformation("Solution Zip Size: {0}", FileUtilities.GetFileSize(solutionFilePath));

            SolutionImportResult result = null;

            SolutionXml solutionXml = new SolutionXml(Logger);

            XrmSolutionInfo info = solutionXml.GetSolutionInfoFromZip(solutionFilePath);

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

            ColumnSet columns = new ColumnSet("version");

            var baseSolution = GetSolution(info.UniqueName, columns);

            if (baseSolution == null)
            {
                Logger.LogInformation("{0} not currently installed.", info.UniqueName);
            }
            else
            {
                Logger.LogInformation("{0} currently installed with version: {1}", info.UniqueName, baseSolution.Version);
            }

            if (baseSolution == null && holdingSolution)
            {
                holdingSolution = false;
                Logger.LogInformation("Setting holdingSolution to false");
            }

            bool skipImport = SkipImport(info, holdingSolution, overrideSameVersion, baseSolution);

            if (skipImport)
            {
                Logger.LogInformation("Solution Import Skipped");

                result = new SolutionImportResult()
                {
                    Success = true,
                    ImportSkipped = true
                };

                result.SolutionName = info.UniqueName;
                result.VersionNumber = info.Version;

                return result;
            }

            if (downloadFormattedLog)
            {
                if (string.IsNullOrEmpty(logFileName))
                {
                    logFileName = $"ImportLog_{Path.GetFileNameWithoutExtension(solutionFilePath)}_{DateTime.Now.ToString("yyyy_MM_dd__HH_mm")}.xml";
                    Logger.LogVerbose("Setting logFileName to {0}", logFileName);
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
            };

            Logger.LogVerbose($"RequestId: {importSolutionRequest.RequestId}");

            //keep seperate to allow compatibility with crm2015
            if (holdingSolution)
                importSolutionRequest.HoldingSolution = holdingSolution;
         

            if (importAsync)
            {
                Logger.LogVerbose(string.Format("Importing solution in Async Mode"));

                var asyncRequest = new ExecuteAsyncRequest
                {
                    Request = importSolutionRequest,
                    RequestId = importSolutionRequest.RequestId
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

            result.SolutionName = info.UniqueName;
            result.VersionNumber = info.Version;

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

        public SolutionImportResult ImportSolution(
            string importFolder,
            string logsFolder,
            SolutionImportOptions options)
        {
            SolutionImportResult importResult = ImportSolution(
                $"{importFolder}\\{options.SolutionFilePath}",
                options.PublishWorkflows,
                options.ConvertToManaged,
                options.OverwriteUnmanagedCustomizations,
                options.SkipProductUpdateDependencies,
                options.HoldingSolution,
                options.OverrideSameVersion,
                options.ImportAsync,
                options.SleepInterval,
                options.AsyncWaitTimeout,
                Guid.NewGuid(),
                true,
                logsFolder,
                string.Empty);

            if (importResult.Success &&
                options.ApplySolution)
            {
                SolutionApplyResult applyResult = ApplySolution(
                    importResult.SolutionName,
                    options.ApplyAsync,
                    options.SleepInterval,
                    options.AsyncWaitTimeout);

                importResult.Success = applyResult.Success;
                importResult.ErrorMessage = applyResult.ErrorMessage;
            }

            return importResult;
        }

        public List<SolutionImportResult> ImportSolutions(
            string importFolder,
            string logsFolder,
            SolutionImportConfig config)
        {
            List<SolutionImportResult> results = new List<SolutionImportResult>();

            foreach (SolutionImportOptions option in config.Solutions)
            {
                SolutionImportResult result = ImportSolution(
                    importFolder,
                    logsFolder,
                    option);

                results.Add(result);

                if (!result.Success)
                {
                    break;
                }
            }

            return results;
        }

        public List<SolutionImportResult> ImportSolutions(
            string logsFolder,
            string configFilePath)
        {
            if (!Directory.Exists(logsFolder))
            {
                throw new Exception($"{logsFolder} does not exist");
            }

            if (!File.Exists(configFilePath))
            {
                throw new Exception($"{configFilePath} does not exist");
            }

            Logger.LogVerbose("Parsing import json file {0}", configFilePath);

            SolutionImportConfig config =
                Serializers.ParseJson<SolutionImportConfig>(configFilePath);

            Logger.LogVerbose("Finished parsing import json file {0}", configFilePath);

            Logger.LogVerbose("{0} solution for import found", config.Solutions.Count);

            FileInfo configInfo = new FileInfo(configFilePath);

            List<SolutionImportResult> results = ImportSolutions(
                configInfo.Directory.FullName,
                logsFolder,
                config);

            return results;
        }

        public SolutionApplyResult ApplySolution(
            string solutionName,
            bool importAsync,
            int sleepInterval,
            int asyncWaitTimeout
            )
        {
            Logger.LogVerbose("Upgrading Solution: {0}", solutionName);

            if (SkipUpgrade(solutionName))
            {
                return new SolutionApplyResult()
                {
                    Success = true,
                    ApplySkipped = true
                };
            }

            Exception syncApplyException = null;
            AsyncOperation asyncOperation = null;

            var upgradeSolutionRequest = new DeleteAndPromoteRequest
            {
                UniqueName = solutionName,
                RequestId = Guid.NewGuid()
            };

            Logger.LogVerbose($"RequestId: {upgradeSolutionRequest.RequestId}");

            if (importAsync)
            {
                var asyncRequest = new ExecuteAsyncRequest
                {
                    Request = upgradeSolutionRequest,
                    RequestId = upgradeSolutionRequest.RequestId
                };

                Logger.LogVerbose("Applying using Async Mode");

                var asyncResponse = OrganizationService.Execute(asyncRequest) as ExecuteAsyncResponse;

                Guid asyncJobId = asyncResponse.AsyncJobId;

                Logger.LogInformation(string.Format("Async JobId: {0}", asyncJobId));

                Logger.LogVerbose("Awaiting for Async Operation Completion");

                AsyncOperationManager asyncOperationManager = new AsyncOperationManager(
                    Logger, PollingOrganizationService);

                asyncOperation = asyncOperationManager.AwaitCompletion(
                    asyncJobId, asyncWaitTimeout, sleepInterval, null);

                Logger.LogInformation("Async Operation completed with status: {0}",
                    ((AsyncOperation_StatusCode)asyncOperation.StatusCode.Value).ToString());

                Logger.LogInformation("Async Operation completed with message: {0}",
                    asyncOperation.Message);
            }
            else
            {
                try
                {
                    OrganizationService.Execute(upgradeSolutionRequest);
                }
                catch (Exception ex)
                {
                    syncApplyException = ex;
                }
            }

            SolutionApplyResult result = VerifyUpgrade(
                solutionName,
                asyncOperation,
                syncApplyException);

            if (result.Success)
            {
                Logger.LogInformation("Solution Apply Completed Successfully");
            }
            else
            {
                Logger.LogInformation("Solution Apply Failed");
            }

            return result;
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
                Logger.LogVerbose($"{uniqueName} solution was not found");

                return null;
            }
            else
            {
                Logger.LogVerbose($"Solution retrieved with Id: {results.Entities[0].Id}");

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
                            select s;

                List<Solution> solutions = query.ToList<Solution>();

                solutions = solutions.OrderBy(s => new Version(s.Version)).Reverse<Solution>().ToList<Solution>();

                return solutions;
            }
        }

        public Solution CreatePatch(string uniqueName,
                                    string versionNumber,
                                    string displayName)
        {

            var solution = GetSolution(uniqueName, new ColumnSet("version", "friendlyname"));

            if (solution == null)
            {
                throw new Exception(string.Format("Solution with unique name {0} not found.", uniqueName));
            }

            if (string.IsNullOrEmpty(versionNumber))
            {
                Logger.LogVerbose("VersionNumber not supplied. Generating default VersionNumber");

                var patches = GetSolutionPatches(uniqueName);

                Version version;
                if (patches.Count == 0)
                {
                    version = new Version(solution.Version);
                }
                else
                {
                    version = new Version(patches[0].Version);
                }

                char dot = '.';
                versionNumber = string.Concat(version.Major, dot, version.Minor, dot, version.Build + 1, dot, 0);
                Logger.LogVerbose("New VersionNumber: {0}", versionNumber);
            }

            if (string.IsNullOrEmpty(displayName))
            {
                Logger.LogVerbose("displayName not supplied. Generating default DisplayName");

                displayName = solution.FriendlyName;

                Logger.LogVerbose("New DisplayName: {0}", displayName);
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

        public string ExportSolution(
            string outputFolder,
            SolutionExportOptions options)
        {
            Logger.LogVerbose("Exporting Solution: {0}", options.SolutionName);

            var solutionFile = new StringBuilder();
            Solution solution = GetSolution(options.SolutionName,
                new ColumnSet("version"));

            if (solution is null)
            {
                throw new Exception($"Unable to find solution with unique name: {options.SolutionName}");
            }
            else
            {
                Logger.LogInformation($"Exporting Solution: {options.SolutionName}, version: {solution.Version}");
            }

            solutionFile.Append(options.SolutionName);

            if (options.IncludeVersionInName)
            {
                solutionFile.Append("_");
                solutionFile.Append(solution.Version.Replace(".", "_"));
            }

            if (options.Managed)
            {
                solutionFile.Append("_managed");
            }

            solutionFile.Append(".zip");

            var exportSolutionRequest = new ExportSolutionRequest
            {
                Managed = options.Managed,
                SolutionName = options.SolutionName,
                ExportAutoNumberingSettings = options.ExportAutoNumberingSettings,
                ExportCalendarSettings = options.ExportCalendarSettings,
                ExportCustomizationSettings = options.ExportCustomizationSettings,
                ExportEmailTrackingSettings = options.ExportEmailTrackingSettings,
                ExportGeneralSettings = options.ExportGeneralSettings,
                ExportIsvConfig = options.ExportIsvConfig,
                ExportMarketingSettings = options.ExportMarketingSettings,
                ExportOutlookSynchronizationSettings = options.ExportOutlookSynchronizationSettings,
                ExportRelationshipRoles = options.ExportRelationshipRoles,
                ExportSales = options.ExportSales,
                TargetVersion = options.TargetVersion,
                RequestId = Guid.NewGuid()
              
            };

            Logger.LogVerbose($"RequestId: {exportSolutionRequest.RequestId}");

            //keep seperate to allow compatibility with crm2015
            if (options.ExportExternalApplications)
                exportSolutionRequest.ExportExternalApplications = options.ExportExternalApplications;

            byte[] solutionBytes;

            if (options.ExportAsync)
            {
                Logger.LogInformation("Exporting Solution using Async Mode");

                exportSolutionRequest.RequestName = "ExportSolutionAsync";

                var asyncExportResponse = OrganizationService.Execute(exportSolutionRequest);

                //Guid asyncJobId = asyncResponse.AsyncJobId;
                Guid asyncJobId = (Guid)asyncExportResponse.Results["AsyncOperationId"];
                Guid exportJobId = (Guid)asyncExportResponse.Results["ExportJobId"];

                Logger.LogInformation($"AsyncOperationId: {asyncJobId}");
                Logger.LogInformation($"ExportJobId: {exportJobId}");

                AsyncOperationManager asyncOperationManager = new AsyncOperationManager(Logger, OrganizationService);
                AsyncOperation operation = asyncOperationManager.AwaitCompletion(asyncJobId, options.AsyncWaitTimeout, options.SleepInterval, null);

                Logger.LogInformation("Async Operation completed with status: {0}",
                    ((AsyncOperation_StatusCode)operation.StatusCode.Value).ToString());
        
                Logger.LogInformation("Async Operation completed with message: {0}",
                    operation.Message);

                if (operation.StatusCode.Value == (int)AsyncOperation_StatusCode.Succeeded)
                {
                    OrganizationRequest downloadReq = new OrganizationRequest("DownloadSolutionExportData");
                    downloadReq.Parameters.Add("ExportJobId", exportJobId);

                    OrganizationResponse downloadRes = OrganizationService.Execute(downloadReq);

                    solutionBytes = (byte[])downloadRes.Results["ExportSolutionFile"];
                }
                else
                {
                    throw new Exception($"Export of solution '{options.SolutionName}' failed: {operation.Message}");
                }
            }
            else
            {
                Logger.LogInformation("Exporting Solution using Sync Mode");

                var exportSolutionResponse = OrganizationService.Execute(exportSolutionRequest) as ExportSolutionResponse;

                solutionBytes = exportSolutionResponse.ExportSolutionFile;
            }

            string solutionFilePath = Path.Combine(outputFolder, solutionFile.ToString());
            File.WriteAllBytes(solutionFilePath, solutionBytes);

            Logger.LogInformation($"Solution Exported to: {solutionFilePath}");
            Logger.LogInformation("Solution Zip Size: {0}", FileUtilities.GetFileSize(solutionFilePath));

            return solutionFilePath;
        }

        public List<string> ExportSolutions(
            string outputFolder,
            SolutionExportConfig config)
        {
            List<string> solutionFilePaths = new List<string>();

            foreach (SolutionExportOptions option in config.Solutions)
            {
                solutionFilePaths.Add(ExportSolution(outputFolder,
                    option));
            }

            return solutionFilePaths;
        }

        public List<string> ExportSolutions(
            string outputFolder,
            string configFilePath)
        {
            SolutionExportConfig config =
                Serializers.ParseJson<SolutionExportConfig>(configFilePath);

            List<string> solutionFilePaths = ExportSolutions(outputFolder,
                config);

            return solutionFilePaths;
        }

        public void UpdateVersion(
            string solutionName,
            string version)
        {
            Logger.LogVerbose("Updating Solution {0} Version: {1}", solutionName, version);

            Solution solution = GetSolution(solutionName, new ColumnSet());

            if (solution == null)
            {
                throw new Exception(string.Format("Solution {0} could not be found", solutionName));
            }

            var update = new Solution
            {
                Id = solution.Id,
                Version = version
            };

            OrganizationService.Update(update);

            Logger.LogInformation("Solution {0} version updated to : {1}", solutionName, version);
        }

        #endregion

        #region Private Methods

        private bool SkipImport(
            XrmSolutionInfo info,
            bool holdingSolution,
            bool overrideSameVersion,
            Solution baseSolution)
        {
            bool skip = false;

            if (baseSolution == null ||
                overrideSameVersion ||
                (info.Version != baseSolution.Version))
            {
                skip = false;
            }
            else
            {
                return true;
            }

            if (holdingSolution)
            {
                string upgradeName = $"{info.UniqueName}_Upgrade";

                var upgradeSolution = GetSolution(upgradeName, new ColumnSet("version"));

                if (upgradeSolution == null)
                {
                    Logger.LogInformation("{0} not currently installed.", upgradeName);
                    skip = false;
                }
                else
                {
                    Logger.LogInformation("{0} currently installed with version: {1}", upgradeName, info.Version);
                    skip = true;
                }
            }

            return skip;
        }

        private bool SkipUpgrade(
            string solutionName)
        {
            string upgradeName = $"{solutionName}_Upgrade";

            var upgradeSolution = GetSolution(upgradeName, new ColumnSet("version"));

            if (upgradeSolution == null)
            {
                Logger.LogInformation("Skipping Upgrade. {0} not currently installed.", upgradeName);
                return true;
            }
            else
            {
                Logger.LogInformation("{0} currently installed with version: {1}", upgradeName, upgradeSolution.Version);
                return false;
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

            if (!string.IsNullOrEmpty(solutionImportError))
            {
                VerifySolutionImport_PrettyPrintErrorMessage(doc, result);
            }

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

            result.Success = solutionImportResult == ImportSuccess;
            if (importAsync)
            {
                result.Success = result.Success && asyncOperation.StatusCodeEnum == AsyncOperation_StatusCode.Succeeded;
            }

            return result;
        }

        public void VerifySolutionImport_PrettyPrintErrorMessage(XmlDocument importJobDoc, SolutionImportResult result)
        {
            try
            {
                Logger.LogVerbose("Check for missing dependencies");
                
                // Find solution import error details
                var missingDependencies = importJobDoc.SelectSingleNode("//solutionManifest/result/parameters/parameter/text()[starts-with(., '<MissingDependencies><MissingDependency>')]");
                if (missingDependencies != null)
                {
                    Logger.LogVerbose("Logging missing dependencies");

                    var errorDoc = new XmlDocument();
                    errorDoc.LoadXml(missingDependencies.Value);

                    //Add information to 'MissingDependencies'
                    {
                        var errorTypeAttributes = errorDoc.SelectNodes("//@type");
                        foreach (XmlAttribute errorTypeAttribute in errorTypeAttributes)
                        {
                            if (Enum.TryParse<ComponentType>(errorTypeAttribute.Value, out var type))
                            {
                                var typeAttribute = errorDoc.CreateAttribute("typeName");
                                typeAttribute.Value = type.ToString();
                                errorTypeAttribute.OwnerElement.Attributes.Append(typeAttribute);
                            }
                        }
                    }

                    var sb = new StringBuilder();
                    var settings = new XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = "  ",
                        NewLineChars = Environment.NewLine,
                        NewLineHandling = NewLineHandling.Replace
                    };
                    using (XmlWriter writer = XmlWriter.Create(sb, settings))
                    {
                        errorDoc.Save(writer);
                    }

                    result.ErrorMessage += Environment.NewLine;
                    result.ErrorMessage += sb.ToString();
                }
                else
                {
                    Logger.LogVerbose("No missing dependencies detected");
                }

            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex.Message);
            }
        }

        private SolutionApplyResult VerifyUpgrade(
            string solutionName,
            AsyncOperation asyncOperation,
            Exception syncApplyException)
        {
            SolutionApplyResult result = new SolutionApplyResult();

            if (asyncOperation != null)
            {
                if ((AsyncOperation_StatusCode)asyncOperation.StatusCode.Value
                    != AsyncOperation_StatusCode.Succeeded)
                {
                    result.Success = false;
                    result.ErrorMessage = asyncOperation.Message;
                    //return result;
                }
            }
            if (syncApplyException != null)
            {
                result.Success = false;
                result.ErrorMessage = syncApplyException.Message;
                //return result;
            }

            string upgradeName = solutionName + "_Upgrade";

            Solution solution = GetSolution(upgradeName, new ColumnSet());

            Logger.LogVerbose("Retrieving Solution: {0}", upgradeName);

            if (solution != null)
            {
                result.Success = false;
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    result.ErrorMessage = string.Format("Solution still exists after upgrade: {0}", upgradeName);
                }
            }
            else
            {
                result.Success = true;
                result.ErrorMessage = string.Empty;
                Logger.LogVerbose("Apply Upgrade completed: {0}", upgradeName);
            }

            return result;
        }

        #endregion
    }

    #region Helper Classes

    public class SolutionImportResult
    {
        #region Properties

        public string SolutionName { get; set; }
        public string VersionNumber { get; set; }
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

    public class SolutionApplyResult
    {
        #region Properties

        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public bool ApplySkipped { get; set; }

        #endregion

        #region Constructor

        public SolutionApplyResult()
        {
            Success = false;
            ApplySkipped = false;
            ErrorMessage = "";
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

    public class SolutionExportConfig
    {
        #region Properties

        public List<SolutionExportOptions> Solutions { get; set; }

        #endregion

        #region Constructors

        public SolutionExportConfig()
        {
            Solutions = new List<SolutionExportOptions>();
        }

        #endregion
    }

    public class SolutionExportOptions
    {
        #region Properties

        public string SolutionName { get; set; }
        public bool Managed { get; set; }
        public string TargetVersion { get; set; }
        public bool ExportAutoNumberingSettings { get; set; }
        public bool ExportCalendarSettings { get; set; }
        public bool ExportCustomizationSettings { get; set; }
        public bool ExportEmailTrackingSettings { get; set; }
        public bool ExportExternalApplications { get; set; }
        public bool ExportGeneralSettings { get; set; }
        public bool ExportIsvConfig { get; set; }
        public bool ExportMarketingSettings { get; set; }
        public bool ExportOutlookSynchronizationSettings { get; set; }
        public bool ExportRelationshipRoles { get; set; }
        public bool ExportSales { get; set; }
        public bool IncludeVersionInName { get; set; }
        public bool ExportAsync { get; set; }
        public int SleepInterval { get; set; }
        public int AsyncWaitTimeout { get; set; }

        #endregion

        #region Constructors

        public SolutionExportOptions()
        {
            SleepInterval = 15;
            AsyncWaitTimeout = 15 * 60;
            ExportAsync = false;
        }

        #endregion
    }

    public class SolutionImportOptions
    {
        #region Properties

        public string SolutionFilePath { get; set; }
        public bool PublishWorkflows { get; set; }
        public bool ConvertToManaged { get; set; }
        public bool OverwriteUnmanagedCustomizations { get; set; }
        public bool SkipProductUpdateDependencies { get; set; }
        public bool HoldingSolution { get; set; }
        public bool OverrideSameVersion { get; set; }
        public bool ImportAsync { get; set; }
        public bool ApplySolution { get; set; }
        public bool ApplyAsync { get; set; }
        public int SleepInterval { get; set; }
        public int AsyncWaitTimeout { get; set; }

        #endregion

        #region Constructors

        public SolutionImportOptions()
        {
            SleepInterval = 15;
            AsyncWaitTimeout = 15 * 60;
            ImportAsync = true;
        }

        #endregion
    }

    public class SolutionImportConfig
    {
        #region Properties

        public List<SolutionImportOptions> Solutions { get; set; }

        #endregion

        #region Constructors

        public SolutionImportConfig()
        {
            Solutions = new List<SolutionImportOptions>();
        }

        #endregion
    }

    #endregion
}
