using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public interface IJobStatusUpdate
    {
        bool OnProgressUpdate(ImportJob importJob);
    }

    public class ImportJobManager : XrmBase
    {
        #region Constructors

        public ImportJobManager(ILogger logger, IOrganizationService organizationService)
            : base(logger, organizationService)
        {
        }

        #endregion

        #region Methods

        public void SaveFormattedLog(Guid importJobId, string logDirectory, string logFileName)
        {
            Logger.LogVerbose("Downloading Import Job Formatted Log for: {0}", importJobId);

            if (!Directory.Exists(logDirectory))
            {
                Logger.LogError("Log Diretory does not exist", logDirectory);
                throw new DirectoryNotFoundException(string.Format("Log Diretory does not exist: {0}", logDirectory));
            }

            var importLogRequest = new RetrieveFormattedImportJobResultsRequest
            {
                ImportJobId = importJobId
            };
            var importLogResponse =
                (RetrieveFormattedImportJobResultsResponse)OrganizationService.Execute(importLogRequest);

            string logFile = string.Format("{0}\\{1}", logDirectory, logFileName);

            Logger.LogVerbose("Writing Import Job Log to: {0}", logFile);

            File.WriteAllText(logFile, importLogResponse.FormattedResults);

            Logger.LogInformation("Import Job Log successfully saved to: {0}", logFile);
        }

        public ImportJob AwaitImportJob(
            Guid importJobId,
            int asyncWaitTimeout,
            int sleepInterval,
            bool waitIfNotFound,
            IJobStatusUpdate statusUpdate
            )
        {
            DateTime end = DateTime.Now.AddSeconds(asyncWaitTimeout);
            ImportJob importJob = null;
            bool completed = false;

            while (!completed)
            {
                if (end < DateTime.Now)
                {
                    throw new Exception(string.Format("Import Timeout Exceeded: {0}", asyncWaitTimeout));
                }

                Logger.LogVerbose(string.Format("Sleeping for {0} seconds", sleepInterval));
                Thread.Sleep(sleepInterval * 1000);

                try
                {
                    importJob = GetImportJob(importJobId,
                        new ColumnSet("importjobid", "completedon", "progress"));

                    if (importJob == null)
                    {
                        Logger.LogVerbose("Unable to find Import Job with Id {0}", importJobId);
                        if (!waitIfNotFound)
                        {
                            completed = true;
                        }
                    }
                    else
                    {
                        Logger.LogVerbose("Import Progress: {0}", importJob.Progress);

                        if (importJob.CompletedOn.HasValue)
                        {
                            Logger.LogVerbose("Completed On: {0}", importJob.CompletedOn);
                            completed = true;
                            break;
                        }
                    }

                    if (statusUpdate != null)
                    {
                        bool continueWaiting = statusUpdate.OnProgressUpdate(importJob);

                        if (!continueWaiting)
                        {
                            Logger.LogVerbose("continueWaiting = false. Existing loop.");

                            completed = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogVerbose(ex.Message);
                }

            } //End of while loop

            return importJob;
        }

        public ImportJob GetImportJob(
                Guid importJobId,
                ColumnSet columns
            )
        {
            QueryByAttribute query = new QueryByAttribute(ImportJob.EntityLogicalName);
            query.AddAttributeValue("importjobid", importJobId);
            query.ColumnSet = columns;
            EntityCollection results = OrganizationService.RetrieveMultiple(query);

            if (results.Entities.Count == 0)
            {
                return null;
            }
            else
            {
                return results.Entities[0].ToEntity<ImportJob>();
            }
        }

        #endregion
    }
}
