using System;
using System.IO;
using System.Management.Automation;
using System.Threading;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.Common;
using Microsoft.Xrm.Sdk;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Upgrades a CRM holding solution</para>
    /// <para type="description">This cmdlet upgrades a CRM holding solution and return AsyncJobId for async upgrades
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Export-XrmSolution -ConnectionString "" -EntityName "account"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsData.Merge, "XrmSolution")]
    public class MergeXrmSolutionCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The unique name of the solution to be exported.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string UniqueSolutionName { get; set; }

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

        public MergeXrmSolutionCommand()
        {
        }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Logger.LogVerbose("Entering MergeXrmSolution");

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

            XrmConnectionManager xrmConnection = new XrmConnectionManager(
                Logger);

            IOrganizationService pollingOrganizationService = xrmConnection.Connect(
                ConnectionString,
                120);

            SolutionManager solutionManager = new SolutionManager(
                Logger,
                OrganizationService,
                pollingOrganizationService);

            SolutionApplyResult result = solutionManager.ApplySolution(
                UniqueSolutionName,
                ImportAsync,
                SleepInterval,
                AsyncWaitTimeout);

            if (!result.Success)
            {
                throw new Exception(string.Format("Solution upgrade Failed. Error: {0}", result.ErrorMessage));
            }

            Logger.LogVerbose("Leaving MergeXrmSolution");

            //base.ProcessRecord();

            //base.WriteVerbose(string.Format("Upgrading Solution: {0}", UniqueSolutionName));

            //if (AsyncWaitTimeout == 0)
            //{
            //    AsyncWaitTimeout = 15 * 60;
            //    base.WriteVerbose(string.Format("Setting Default AsyncWaitTimeout: {0}", AsyncWaitTimeout));
            //}

            //if (SleepInterval == 0)
            //{
            //    SleepInterval = 15;
            //    base.WriteVerbose(string.Format("Setting Default SleepInterval: {0}", SleepInterval));
            //}

            //var upgradeSolutionRequest = new DeleteAndPromoteRequest
            //{
            //    UniqueName = UniqueSolutionName
            //};

            //if (ImportAsync)
            //{
            //    var asyncRequest = new ExecuteAsyncRequest
            //    {
            //        Request = upgradeSolutionRequest
            //    };

            //    base.WriteVerbose("Applying using Async Mode");

            //    var asyncResponse = OrganizationService.Execute(asyncRequest) as ExecuteAsyncResponse;

            //    Guid asyncJobId = asyncResponse.AsyncJobId;

            //    base.WriteVerbose(string.Format("Async JobId: {0}", asyncJobId));

            //    WriteObject(asyncJobId);

            //    if (WaitForCompletion)
            //    {
            //        base.WriteVerbose("Waiting for completion");
            //        AwaitCompletion(asyncJobId);
            //    }
            //}
            //else
            //{
            //    OrganizationService.Execute(upgradeSolutionRequest);
            //}

            //base.WriteVerbose(string.Format("{0} Upgrade Completed", UniqueSolutionName));

            //VerifyUpgrade();
        }

        //private void AwaitCompletion(Guid asyncJobId)
        //{
        //    DateTime end = DateTime.Now.AddSeconds(AsyncWaitTimeout);

        //    bool completed = false;
        //    while (!completed)
        //    {
        //        if (end < DateTime.Now)
        //        {
        //            throw new Exception(string.Format("Import Timeout Exceeded: {0}", AsyncWaitTimeout));
        //        }

        //        Thread.Sleep(SleepInterval * 1000);
        //        base.WriteVerbose(string.Format("Sleeping for {0} seconds", SleepInterval));

        //        AsyncOperation asyncOperation;

        //        try
        //        {
        //            asyncOperation = OrganizationService.Retrieve("asyncoperation", asyncJobId,
        //                new ColumnSet("asyncoperationid", "statuscode", "message")).ToEntity<AsyncOperation>();
        //        }
        //        catch (Exception ex)
        //        {
        //            base.WriteWarning(ex.Message);
        //            continue;
        //        }

        //        base.WriteVerbose(string.Format("StatusCode: {0}", asyncOperation.StatusCode.Value));

        //        switch (asyncOperation.StatusCode.Value)
        //        {
        //            //Succeeded
        //            case 30:
        //                completed = true;
        //                break;
        //            //Pausing //Canceling //Failed //Canceled
        //            case 21:
        //            case 22:
        //            case 31:
        //            case 32:
        //                throw new Exception(string.Format("Solution Apply with asyncJobId {0} Failed: {1} {2}",
        //                    asyncJobId, asyncOperation.StatusCode.Value, asyncOperation.Message));
        //        }
        //    }
        //}


        //private void VerifyUpgrade()
        //{
        //    string upgradeName = UniqueSolutionName + "_Upgrade";

        //    base.WriteVerbose(string.Format("Retrieving Solution: {0}", upgradeName));

        //    using (var context = new CIContext(OrganizationService))
        //    {
        //        var query = from s in context.SolutionSet
        //                    where s.UniqueName == upgradeName
        //                    select s;

        //        Solution solution = query.FirstOrDefault();

        //        if (solution != null)
        //        {
        //            throw new Exception(string.Format("Solution still exists after upgrade: {0}", upgradeName));
        //        }
        //        else
        //        {
        //            base.WriteVerbose(string.Format("Upgrade Solution Merged: {0}", upgradeName));
        //        }
        //    }
        //}

        #endregion
    }
}