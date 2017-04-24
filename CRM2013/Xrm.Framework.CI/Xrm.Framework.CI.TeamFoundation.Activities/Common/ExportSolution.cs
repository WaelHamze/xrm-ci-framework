using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using System.IO;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.Common
{
    public class ExportSolution
    {
        #region Properties

        public IOrganizationService OrgService { get; private set; }

        #endregion

        #region Constructors

        public ExportSolution(IOrganizationService orgService)
        {
            OrgService = orgService;
        }

        #endregion

        #region Methods

        public string Export(bool managed,
                                string outputFolder,
                                string solutionName,
                                bool exportAutoNumberingSettings,
                                bool exportCalendarSettings,
                                bool exportCustomizationSettings,
                                bool exportEmailTrackingSettings,
                                bool exportGeneralSettings,
                                bool exportIsvConfig,
                                bool exportMarketingSettings,
                                bool exportOutlookSynchronizationSettings,
                                bool exportRelationshipRoles)
        {
            StringBuilder solutionFile = new StringBuilder();
            Solution solution = null;

            using (CIContext context = new CIContext(OrgService))
            {
                var query = from s in context.SolutionSet
                            where s.UniqueName == solutionName
                            select s;

                solution = query.FirstOrDefault<Solution>();            
            }

            if (solution == null)
            {
                throw new Exception(string.Format("Solution {0} could not be found", solutionName));
            }
            else
            {
                solutionFile.Append(solutionName);
                solutionFile.Append("_");

                solutionFile.Append(solution.Version.Replace(".", "_"));
            }

            if (managed)
            {
                solutionFile.Append("_managed");
            }

            solutionFile.Append(".zip");

            ExportSolutionRequest req = new ExportSolutionRequest()
            {
                Managed = managed,
                SolutionName = solutionName,
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

            ExportSolutionResponse res = OrgService.Execute(req) as ExportSolutionResponse;

            File.WriteAllBytes(outputFolder + "\\" + solutionFile.ToString(), res.ExportSolutionFile);

            return solutionFile.ToString();
        }

        #endregion
    }
}
