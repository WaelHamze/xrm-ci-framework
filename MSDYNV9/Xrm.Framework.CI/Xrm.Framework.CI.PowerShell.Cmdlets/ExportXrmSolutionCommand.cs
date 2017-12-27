using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
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

        public ExportXrmSolutionCommand()
        {
            IncludeVersionInName = false;
        }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Exporting Solution: {0}", UniqueSolutionName));

            var solutionFile = new StringBuilder();
            Solution solution;

            using (var context = new CIContext(OrganizationService))
            {
                var query = from s in context.SolutionSet
                            where s.UniqueName == UniqueSolutionName
                            select s;

                solution = query.FirstOrDefault();
            }

            if (solution == null)
            {
                throw new Exception(string.Format("Solution {0} could not be found", UniqueSolutionName));
            }
            solutionFile.Append(UniqueSolutionName);

            if (IncludeVersionInName)
            {
                solutionFile.Append("_");
                solutionFile.Append(solution.Version.Replace(".", "_"));
            }

            if (Managed)
            {
                solutionFile.Append("_managed");
            }

            solutionFile.Append(".zip");

            var exportSolutionRequest = new ExportSolutionRequest
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
                ExportExternalApplications = ExportExternalApplications
            };

            var exportSolutionResponse = OrganizationService.Execute(exportSolutionRequest) as ExportSolutionResponse;

            string solutionFilePath = Path.Combine(OutputFolder, solutionFile.ToString());
            File.WriteAllBytes(solutionFilePath, exportSolutionResponse.ExportSolutionFile);

            base.WriteObject(solutionFile.ToString());
        }

        #endregion
    }
}