using System;
using System.IO;
using System.Management.Automation;
using System.Threading;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.PowerShell.Command
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
            AsyncWaitTimeout = 15 * 60;
            SleepInterval = 15;
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
                RequestId = ImportJobId
            };

            if (ImportAsync)
            {
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
                    AwaitCompletion(asyncJobId);
                }
            }
            else
            {
                OrganizationService.Execute(importSolutionRequest);
            }

            base.WriteVerbose(string.Format("{0} Imported Completed {1}", SolutionFilePath, ImportJobId));
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