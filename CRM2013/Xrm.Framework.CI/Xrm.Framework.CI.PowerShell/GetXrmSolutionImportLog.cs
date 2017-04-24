using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Xrm.Framework.CI.Common;
using Microsoft.Crm.Sdk.Messages;
using System.IO;
using Xrm.Framework.CI.Common.Entities;
using Microsoft.Xrm.Sdk.Query;

namespace Xrm.Framework.CI.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "XrmSolutionImportLog")]
    public class GetXrmSolutionImportLogCommand : Cmdlet
    {
        #region Parameters

        [Parameter(Mandatory = true)]
        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }
        private string connectionString;

        [Parameter(Mandatory = true)]
        public Guid ImportJobId
        {
            get { return importJobId; }
            set { importJobId = value; }
        }
        private Guid importJobId;

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

            CrmConnection connection = CrmConnection.Parse(connectionString);

            using (OrganizationService service = new OrganizationService(connection))
            {
                ImportJob importJob = service.Retrieve(ImportJob.EntityLogicalName, importJobId, new ColumnSet(true)).ToEntity<ImportJob>();
                
                RetrieveFormattedImportJobResultsRequest importLogRequest = new RetrieveFormattedImportJobResultsRequest()
                {
                    ImportJobId = importJobId
                };
                RetrieveFormattedImportJobResultsResponse importLogResponse =
                    (RetrieveFormattedImportJobResultsResponse)service.Execute(importLogRequest);

                if (!string.IsNullOrEmpty(outputFile))
                {
                    File.WriteAllText(outputFile, importLogResponse.FormattedResults);
                }

                WriteObject(importJob);
            }

            base.WriteVerbose(string.Format("Solution Import Log Downloaded Successfully"));
        }

        #endregion
    }
}
