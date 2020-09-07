using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Exports a CRM Solution.</para>
    /// <para type="description">The Export-XrmSolution exports a CRM solution by unique name
    ///  and return the exported solution file name.
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Export-XrmSolution -ConnectionString "" -EntityName "account"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsData.Export, "XrmSolution")]
    [OutputType(typeof(String))]
    public class ExportXrmSolutionCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The unique name of the solution to be exported.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string UniqueSolutionName { get; set; }

        /// <summary>
        /// <para type="description">Whether the solution to be exported is a managed solution</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public bool Managed { get; set; }

        /// <summary>
        /// <para type="description">As per ExportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.exportsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string TargetVersion { get; set; }

        /// <summary>
        /// <para type="description">The absolute path to the location of the exported solution</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string OutputFolder { get; set; }

        /// <summary>
        /// <para type="description">Whether to include solution version number in the exported solution file name</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool IncludeVersionInName { get; set; }

        /// <summary>
        /// <para type="description">As per ExportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.exportsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool ExportAutoNumberingSettings { get; set; }

        /// <summary>
        /// <para type="description">As per ExportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.exportsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool ExportCalendarSettings { get; set; }

        /// <summary>
        /// <para type="description">As per ExportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.exportsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool ExportCustomizationSettings { get; set; }

        /// <summary>
        /// <para type="description">As per ExportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.exportsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool ExportEmailTrackingSettings { get; set; }

        /// <summary>
        /// <para type="description">As per ExportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.exportsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool ExportExternalApplications { get; set; }

        /// <summary>
        /// <para type="description">As per ExportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.exportsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool ExportGeneralSettings { get; set; }

        /// <summary>
        /// <para type="description">As per ExportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.exportsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool ExportIsvConfig { get; set; }

        /// <summary>
        /// <para type="description">As per ExportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.exportsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool ExportMarketingSettings { get; set; }

        /// <summary>
        /// <para type="description">As per ExportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.exportsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool ExportOutlookSynchronizationSettings { get; set; }

        /// <summary>
        /// <para type="description">As per ExportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.exportsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool ExportRelationshipRoles { get; set; }

        /// <summary>
        /// <para type="description">As per ExportSolutionRequest (see https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.exportsolutionrequest_properties.aspx )</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool ExportSales { get; set; }

        /// <summary>
        /// <para type="description">Specify whether to import the solution asynchronously using ExecuteAsyncRequest</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool ExportAsync { get; set; }

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

        public ExportXrmSolutionCommand()
        {
            IncludeVersionInName = false;
            SleepInterval = 15;
            AsyncWaitTimeout = 15 * 60;
        }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Logger.LogVerbose("Entering XrmExportSolution");

            XrmConnectionManager xrmConnection = new XrmConnectionManager(
                Logger);

            SolutionManager solutionManager = new SolutionManager(
                Logger,
                OrganizationService,
                null);

            SolutionExportOptions options = new SolutionExportOptions
            {
                Managed = Managed,
                SolutionName = UniqueSolutionName,
                ExportAutoNumberingSettings = ExportAutoNumberingSettings,
                ExportCalendarSettings = ExportCalendarSettings,
                ExportCustomizationSettings = ExportCustomizationSettings,
                ExportEmailTrackingSettings = ExportEmailTrackingSettings,
                ExportGeneralSettings = ExportGeneralSettings,
                ExportIsvConfig = ExportIsvConfig,
                ExportMarketingSettings = ExportMarketingSettings,
                ExportOutlookSynchronizationSettings = ExportOutlookSynchronizationSettings,
                ExportRelationshipRoles = ExportRelationshipRoles,
                ExportSales = ExportSales,
                TargetVersion = TargetVersion,
                ExportExternalApplications = ExportExternalApplications,
                IncludeVersionInName = IncludeVersionInName,
                ExportAsync = ExportAsync,
                AsyncWaitTimeout = AsyncWaitTimeout,
                SleepInterval = SleepInterval
            };

            string solutionFile = solutionManager.ExportSolution(
                OutputFolder,
                options);

            base.WriteObject(solutionFile);

            Logger.LogVerbose("Leaving XrmExportSolution");
        }

        #endregion
    }
}