using System;
using System.IO;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves the solution import log file.</para>
    /// <para type="description">This cmdlet retrieves the CRM solution import log file
    ///  using the import job id and return the strongly typed ImportJob object.
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Export-XrmSolution -ConnectionString "" -EntityName "account"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "XrmSolutionImportLog")]
    [OutputType(typeof(ImportJob))]
    public class GetXrmSolutionImportLogCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The unique id of the ImportJob.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public Guid ImportJobId
        {
            get { return importJobId; }
            set { importJobId = value; }
        }

        private Guid importJobId;

        /// <summary>
        /// <para type="description">The absolute path the file where the log should be placed</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string OutputFile
        {
            get { return outputFile; }
            set { outputFile = value; }
        }

        private string outputFile;

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Downloading Solution Import Log for: {0}", ImportJobId));

            var importJob = OrganizationService.Retrieve(ImportJob.EntityLogicalName, importJobId, new ColumnSet(true)).ToEntity<ImportJob>();

            var importLogRequest = new RetrieveFormattedImportJobResultsRequest
            {
                ImportJobId = importJobId
            };
            var importLogResponse =
                (RetrieveFormattedImportJobResultsResponse)OrganizationService.Execute(importLogRequest);

            if (!string.IsNullOrEmpty(outputFile))
            {
                File.WriteAllText(outputFile, importLogResponse.FormattedResults);
            }

            WriteObject(importJob);

            base.WriteVerbose(string.Format("Solution Import Log Downloaded Successfully"));
        }

        #endregion
    }
}
