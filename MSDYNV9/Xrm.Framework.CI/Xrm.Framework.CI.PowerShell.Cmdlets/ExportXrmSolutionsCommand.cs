using System;
using System.Collections.Generic;
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
    [Cmdlet(VerbsData.Export, "XrmSolutions")]
    [OutputType(typeof(String))]
    public class ExportXrmSolutionsCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The absolute path to the json config file containing export configuration</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string ConfigFilePath { get; set; }

        /// <summary>
        /// <para type="description">The absolute path to the location of the exported solution</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string OutputFolder { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Logger.LogVerbose("Entering XrmExportSolutions");

            XrmConnectionManager xrmConnection = new XrmConnectionManager(
                Logger);

            SolutionManager solutionManager = new SolutionManager(
                Logger,
                OrganizationService,
                null);

            List<string> solutions = solutionManager.ExportSolutions(
                OutputFolder,
                ConfigFilePath);

            base.WriteObject(solutions);

            Logger.LogVerbose("Leaving XrmExportSolutions");
        }

        #endregion
    }
}