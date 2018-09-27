using System;
using System.IO;
using System.Management.Automation;
using System.Threading;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Imports a CRM solution</para>
    /// <para type="description">This cmdlet imports a CRM solution and return AsyncJobId for async imports
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Export-XrmSolution -ConnectionString "" -EntityName "account"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsData.Import, "XrmSolution")]
    public class ImportXrmSolutionCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The absolute path to the solution file to be imported</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string SolutionFilePath { get; set; }

        /// <summary>
        /// <para type="description">As per ImportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.importsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool PublishWorkflows { get; set; }

        /// <summary>
        /// <para type="description">As per ImportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.importsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool ConvertToManaged { get; set; }

        /// <summary>
        /// <para type="description">As per ImportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.importsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool OverwriteUnmanagedCustomizations { get; set; }

        /// <summary>
        /// <para type="description">As per ImportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.importsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)] 
        public bool SkipProductUpdateDependencies { get; set; }

        /// <summary>
        /// <para type="description">As per ImportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.importsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool HoldingSolution { get; set; }

        /// <summary>
        /// <para type="description">Specify whether to import the solution asynchronously using ExecuteAsyncRequest</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool ImportAsync { get; set; }

        /// <summary>
        /// <para type="description">Specify whether to wait for async solution imports</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool WaitForCompletion { get; set; }

        /// <summary>
        /// <para type="description">The sleep interval between checks on the import progress. Default = 15 seconds</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int SleepInterval { get; set; }

        /// <summary>
        /// <para type="description">Specify the timeout duration for waiting on async imports to complete. Default = 15 minutes</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int AsyncWaitTimeout { get; set; }

        /// <summary>
        /// <para type="description">Specify the Guid to be used for the async import job. This was be used to query the start of the job after.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public Guid ImportJobId { get; set; }

        public ImportXrmSolutionCommand()
        {
        }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Importing Solution: {0}", SolutionFilePath));

            // TODO: I think this is not necessary because you will get back an Id if you overload Guid.Empty
            if (ImportJobId == Guid.Empty)
            {
                ImportJobId = Guid.NewGuid();
            }

            if (AsyncWaitTimeout == 0)
            {
                AsyncWaitTimeout = 15 * 60;
                base.WriteVerbose(string.Format("Setting Default AsyncWaitTimeout: {0}", AsyncWaitTimeout));
            }

            if (SleepInterval == 0)
            {
                SleepInterval = 15;
                base.WriteVerbose(string.Format("Setting Default SleepInterval: {0}", SleepInterval));
            }

            base.WriteVerbose(string.Format("ImportJobId {0}", ImportJobId));

            byte[] solutionBytes = File.ReadAllBytes(SolutionFilePath);

            var importSolutionRequest = new ImportSolutionRequest
            {
                CustomizationFile = solutionBytes,
                PublishWorkflows = PublishWorkflows,
                ConvertToManaged = ConvertToManaged,
                OverwriteUnmanagedCustomizations = OverwriteUnmanagedCustomizations,
                SkipProductUpdateDependencies = SkipProductUpdateDependencies,
                ImportJobId = ImportJobId,
                RequestId = ImportJobId,
                HoldingSolution = HoldingSolution
            };

            if (ImportAsync)
            {
                base.WriteVerbose(string.Format("Importing solution in Async Mode"));

                var asyncRequest = new ExecuteAsyncRequest
                {
                    Request = importSolutionRequest,
                    RequestId = ImportJobId
                };
                var asyncResponse = OrganizationService.Execute(asyncRequest) as ExecuteAsyncResponse;

                Guid asyncJobId = asyncResponse.AsyncJobId;

                WriteObject(asyncJobId);

                if (WaitForCompletion)
                {
                    base.WriteVerbose(string.Format("Awaiting for Async Operation Completion"));

                    AwaitCompletion(asyncJobId);
                }
            }
            else
            {
                base.WriteVerbose(string.Format("Importing solution in Sync Mode"));

                try
                {
                    OrganizationService.Execute(importSolutionRequest);
                }
                catch(Exception ex)
                {
                    if (WaitForCompletion)
                    {
                        base.WriteWarning(ex.Message);

                        base.WriteVerbose("Exception Handled. Attempting to Wait for ImportJob to Complete.");

                        AwaitImportJob();
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

            base.WriteVerbose(string.Format("{0} Imported Completed {1}", SolutionFilePath, ImportJobId));
        }

        private void AwaitImportJob()
        {
            DateTime end = DateTime.Now.AddSeconds(AsyncWaitTimeout);

            bool completed = false;
            bool notFound = false;
            while (!completed)
            {
                if (end < DateTime.Now)
                {
                    throw new Exception(string.Format("Import Timeout Exceeded: {0}", AsyncWaitTimeout));
                }

                base.WriteVerbose(string.Format("Sleeping for {0} seconds", SleepInterval));
                Thread.Sleep(SleepInterval * 1000);

                ImportJob importJob;

                try
                {
                    QueryByAttribute query = new QueryByAttribute(ImportJob.EntityLogicalName);
                    query.AddAttributeValue("importjobid", ImportJobId);
                    query.ColumnSet = new ColumnSet("importjobid", "completedon", "progress");

                    EntityCollection results = OrganizationService.RetrieveMultiple(query);

                    if (results.TotalRecordCount == 0)
                    {
                        completed = true;
                        notFound = true;
                        break;
                    }
                    else
                    {
                        importJob = results.Entities[0].ToEntity<ImportJob>();
                    }
                }
                catch (Exception ex)
                {
                    base.WriteWarning(ex.Message);
                    continue;
                }

                base.WriteVerbose(string.Format("Progress: {0}", importJob.Progress));
                base.WriteVerbose(string.Format("CompletedOn: {0}", importJob.CompletedOn));

                if (importJob.CompletedOn.HasValue)
                {
                    completed = true;
                    break;
                }
            }

            if (notFound)
            {
                throw new Exception(String.Format("Unable to find Import Job with Id {0}", ImportJobId));
            }
        }

        private void AwaitCompletion(Guid asyncJobId)
        {
            DateTime end = DateTime.Now.AddSeconds(AsyncWaitTimeout);

            bool completed = false;
            while (!completed)
            {
                if (end < DateTime.Now)
                {
                    throw new Exception(string.Format("Import Timeout Exceeded: {0}", AsyncWaitTimeout));
                }

                base.WriteVerbose(string.Format("Sleeping for {0} seconds", SleepInterval));
                Thread.Sleep(SleepInterval * 1000);

                AsyncOperation asyncOperation;

                try
                {
                    asyncOperation = OrganizationService.Retrieve("asyncoperation", asyncJobId,
                        new ColumnSet("asyncoperationid", "statuscode", "message")).ToEntity<AsyncOperation>();
                }
                catch (Exception ex)
                {
                    base.WriteWarning(ex.Message);
                    continue;
                }

                base.WriteVerbose(string.Format("StatusCode: {0}", asyncOperation.StatusCode.Value));

                switch (asyncOperation.StatusCode.Value)
                {
                    //Succeeded
                    case 30:
                        completed = true;
                        break;
                    //Pausing //Canceling //Failed //Canceled
                    case 21:
                    case 22:
                    case 31:
                    case 32:
                        throw new Exception(string.Format("Solution Import Failed: {0} {1}",
                            asyncOperation.StatusCode.Value, asyncOperation.Message));
                }
            }
        }

        #endregion
    }
}