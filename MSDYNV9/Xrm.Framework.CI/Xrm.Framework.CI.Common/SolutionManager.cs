using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
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

        #region Constructors

        public SolutionManager(ILogger logger, IOrganizationService organizationService)
            :base(logger, organizationService)
        {
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
            bool importAsync,
            int sleepInterval,
            int asyncWaitTimeout,
            Guid? importJobId,
            bool downloadFormattedLog,
            string logDirectory,
            string logFileName
            )
        {
            Logger.LogVerbose("Importing Solution: {0}", solutionFilePath);

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

            if (downloadFormattedLog)
            {
                if (string.IsNullOrEmpty(logFileName))
                {
                    Logger.LogError("logFileName is required", solutionFilePath);
                    throw new Exception("logFileName is required");
                }

                if (!Directory.Exists(logDirectory))
                {
                    Logger.LogError("logDirectory not exist: {0}", logDirectory);
                    throw new DirectoryNotFoundException("logDirectory does not exist");
                }
            }

            SolutionImportResult result = null;

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
                    Logger, OrganizationService, importJobId.Value);

                AsyncOperationManager operationManager
                    = new AsyncOperationManager(Logger, OrganizationService);

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

                Logger.LogVerbose("Creating Import Thread");

                Thread importThread = new Thread(
                    () => importHandler.ImportSolution()
                    );
                Logger.LogVerbose("Starting Import Thread");

                importThread.Start();

                Logger.LogVerbose("Thread Started. Starting to Query Import Status");

                ImportJobManager jobManager = new ImportJobManager(Logger, OrganizationService);
                jobManager.AwaitImportJob(importJobId.Value, asyncWaitTimeout, sleepInterval, true, jobHandler);

                importThread.Join();

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

            Logger.LogInformation("Completed Progress: {0}", importJob.Progress);
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
                Logger.LogVerbose("Total number of unprocessed components: {0}", unprocessedNodes.Count);
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

        #endregion

        #region Constructor

        public SolutionImportResult()
        {
            Success = false;
            ErrorMessage = "";
            UnprocessedComponents = -1;
            ImportJobAvailable = false;
        }

        #endregion
    }

    class SyncImportHandler : XrmBase
    {
        #region Properties

        private ImportSolutionRequest ImportRequest
        {
            get;
            set;
        }

        public bool Finished
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
                Logger.LogInformation("Calling Execute Import Request");

                Thread.Sleep(30 * 1000);

                OrganizationService.Execute(ImportRequest);

                Logger.LogInformation("Synchronous Solution Import Request Completed");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                Error = ex;
            }

            Finished = true;
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

                Logger.LogVerbose("Import Job Progress: {0}", importJob.Progress);
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
                return false;
            }

            return true;
        }

        #endregion
    }

    #endregion
}
