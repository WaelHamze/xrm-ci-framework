using System;
using System.Collections.Generic;
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
    [Cmdlet(VerbsData.Import, "XrmSolutions")]
    public class ImportXrmSolutionsCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The absolute path to the json file containing exported config</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string ConfigFilePath { get; set; }

        /// <summary>
        /// <para type="description">The directory where the formatted log should be placed.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string LogsDirectory { get; set; }

        public ImportXrmSolutionsCommand()
        {
        }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Logger.LogVerbose("Entering XrmImportSolutions");

            Logger.LogVerbose("Creating Polling Service");

            XrmConnectionManager xrmConnection = new XrmConnectionManager(
                Logger);

            IOrganizationService pollingOrganizationService = xrmConnection.Connect(
                ConnectionString,
                120);

            Logger.LogVerbose("Finished Creating Polling Service");

            SolutionManager solutionManager = new SolutionManager(
                Logger,
                OrganizationService,
                pollingOrganizationService);

            Logger.LogVerbose("Starting to proccess solutions");

            List<SolutionImportResult> results = solutionManager.ImportSolutions(
                LogsDirectory,
                ConfigFilePath);

            base.WriteObject(results);

            Logger.LogVerbose("All solutions processed");

            List<SolutionImportResult> failed = results.FindAll(r => r.Success.Equals(false));

            Logger.LogVerbose("Failed results count: {0}", failed.Count);

            if (failed.Count > 0)
            {
                throw new Exception(string.Format("Solution import Failed"));
            }

            Logger.LogVerbose("Leaving XrmImportSolutions");
        }

        #endregion
    }
}