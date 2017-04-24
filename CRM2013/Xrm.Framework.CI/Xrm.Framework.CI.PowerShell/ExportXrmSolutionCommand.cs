using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;
using Microsoft.Crm.Sdk.Messages;
using System.IO;

namespace Xrm.Framework.CI.PowerShell
{
    [Cmdlet(VerbsData.Export, "XrmSolution")]
    public class ExportXrmSolutionCommand : Cmdlet
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
        public string UniqueSolutionName
        {
            get { return uniqueSolutionName; }
            set { uniqueSolutionName = value; }
        }
        private string uniqueSolutionName;

        [Parameter(Mandatory = true)]
        public bool Managed
        {
            get { return managed; }
            set { managed = value; }
        }
        private bool managed = false;

        [Parameter(Mandatory = true)]
        public string OutputFolder
        {
            get { return outputFolder; }
            set { outputFolder = value; }
        }
        private string outputFolder;

        [Parameter(Mandatory = false)]
        public bool IncludeVersionInName
        {
            get { return includeVersionInName; }
            set { includeVersionInName = value; }
        }
        private bool includeVersionInName = false;

        [Parameter(Mandatory = false)]
        public bool ExportAutoNumberingSettings
        {
            get { return exportAutoNumberingSettings; }
            set { exportAutoNumberingSettings = value; }
        }
        private bool exportAutoNumberingSettings = false;

        [Parameter(Mandatory = false)]
        public bool ExportCalendarSettings
        {
            get { return exportCalendarSettings; }
            set { exportCalendarSettings = value; }
        }
        private bool exportCalendarSettings = false;

        [Parameter(Mandatory = false)]
        public bool ExportCustomizationSettings
        {
            get { return exportCustomizationSettings; }
            set { exportCustomizationSettings = value; }
        }
        private bool exportCustomizationSettings = false;

        [Parameter(Mandatory = false)]
        public bool ExportEmailTrackingSettings
        {
            get { return exportEmailTrackingSettings; }
            set { exportEmailTrackingSettings = value; }
        }
        private bool exportEmailTrackingSettings = false;

        [Parameter(Mandatory = false)]
        public bool ExportGeneralSettings
        {
            get { return exportGeneralSettings; }
            set { exportGeneralSettings = value; }
        }
        private bool exportGeneralSettings = false;

        [Parameter(Mandatory = false)]
        public bool ExportIsvConfig
        {
            get { return exportIsvConfig; }
            set { exportIsvConfig = value; }
        }
        private bool exportIsvConfig = false;

        [Parameter(Mandatory = false)]
        public bool ExportMarketingSettings
        {
            get { return exportMarketingSettings; }
            set { exportMarketingSettings = value; }
        }
        private bool exportMarketingSettings = false;

        [Parameter(Mandatory = false)]
        public bool ExportOutlookSynchronizationSettings
        {
            get { return exportOutlookSynchronizationSettings; }
            set { exportOutlookSynchronizationSettings = value; }
        }
        private bool exportOutlookSynchronizationSettings = false;

        [Parameter(Mandatory = false)]
        public bool ExportRelationshipRoles
        {
            get { return exportRelationshipRoles; }
            set { exportRelationshipRoles = value; }
        }
        private bool exportRelationshipRoles = false;



        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Exporting Solution: {0}", UniqueSolutionName));

            CrmConnection connection = CrmConnection.Parse(connectionString);

            using (OrganizationService service = new OrganizationService(connection))
            {
                StringBuilder solutionFile = new StringBuilder();
                Solution solution = null;

                using (CIContext context = new CIContext(service))
                {
                    var query = from s in context.SolutionSet
                                where s.UniqueName == UniqueSolutionName
                                select s;

                    solution = query.FirstOrDefault<Solution>();
                }

                if (solution == null)
                {
                    throw new Exception(string.Format("Solution {0} could not be found", UniqueSolutionName));
                }
                else
                {
                    solutionFile.Append(UniqueSolutionName);
                    
                    if (IncludeVersionInName)
                    {
                        solutionFile.Append("_");
                        solutionFile.Append(solution.Version.Replace(".", "_"));
                    }
                }

                if (managed)
                {
                    solutionFile.Append("_managed");
                }

                solutionFile.Append(".zip");

                ExportSolutionRequest req = new ExportSolutionRequest()
                {
                    Managed = managed,
                    SolutionName = UniqueSolutionName,
                    ExportAutoNumberingSettings = exportAutoNumberingSettings,
                    ExportCalendarSettings = exportCalendarSettings,
                    ExportCustomizationSettings = exportCustomizationSettings,
                    ExportEmailTrackingSettings = exportEmailTrackingSettings,
                    ExportGeneralSettings = exportGeneralSettings,
                    ExportIsvConfig = exportIsvConfig,
                    ExportMarketingSettings = exportMarketingSettings,
                    ExportOutlookSynchronizationSettings = exportOutlookSynchronizationSettings,
                    ExportRelationshipRoles = exportRelationshipRoles
                };

                ExportSolutionResponse res = service.Execute(req) as ExportSolutionResponse;

                File.WriteAllBytes(outputFolder + "\\" + solutionFile.ToString(), res.ExportSolutionFile);

                base.WriteObject(solutionFile.ToString());
            }
        }

        #endregion
    }
}
