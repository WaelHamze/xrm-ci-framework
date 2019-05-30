using System;
using System.IO;
using System.Management.Automation;
using System.Threading;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xrm.Framework.CI.Common;
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
        /// <para type="description">Set to true to import solution even if solution with same version is already installed.<para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool OverrideSameVersion { get; set; } = true;

        /// <summary>
        /// <para type="description">Specify whether to import the solution asynchronously using ExecuteAsyncRequest</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool ImportAsync { get; set; }

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

        /// <summary>
        /// <para type="description">Specify whether to download the formatted solution import log.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool DownloadFormattedLog { get; set; }

        /// <summary>
        /// <para type="description">The directory where the formatted log should be placed.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string LogsDirectory { get; set; }

        /// <summary>
        /// <para type="description">The filename for the formatted log. This file doesn't need to exist.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string LogFileName { get; set; }

        public ImportXrmSolutionCommand()
        {
        }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Logger.LogVerbose("Entering XrmImportSolution");

            XrmConnectionManager xrmConnection = new XrmConnectionManager(
                Logger);

            IOrganizationService pollingOrganizationService = xrmConnection.Connect(
                ConnectionString,
                120);

            SolutionManager solutionManager = new SolutionManager(
                Logger,
                OrganizationService,
                pollingOrganizationService);

            SolutionImportResult result = solutionManager.ImportSolution(
                SolutionFilePath,
                PublishWorkflows,
                ConvertToManaged,
                OverwriteUnmanagedCustomizations,
                SkipProductUpdateDependencies,
                HoldingSolution,
                OverrideSameVersion,
                ImportAsync,
                SleepInterval,
                AsyncWaitTimeout,
                ImportJobId,
                DownloadFormattedLog,
                LogsDirectory,
                LogFileName);

            if (!result.Success)
            {
                throw new Exception(string.Format("Solution import Failed. Error: {0}", result.ErrorMessage));
            }

            Logger.LogVerbose("Leaving XrmImportSolution");
        }

        #endregion
    }
}