using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Client.Services;
using System.ComponentModel;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.Xrm.Client;

namespace Xrm.Framework.CI.TeamFoundation.Activities
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public sealed class ExportSolution : CodeActivity
    {
        #region Arguments

        [RequiredArgument]
        public InArgument<string> CrmConnectionString { get; set; }

        [RequiredArgument]
        public InArgument<string> UniqueSolutionName { get; set; }

        [RequiredArgument]
        public InArgument<string> OutputFolder { get; set; }

        [DefaultValue(false)]
        public InArgument<bool> Managed { get; set; }

        [DefaultValue(false)]
        public InArgument<bool> ExportAutoNumberingSettings { get; set; }

        [DefaultValue(false)]
        public InArgument<bool> ExportCalendarSettings { get; set; }

        [DefaultValue(false)]
        public InArgument<bool> ExportCustomizationSettings { get; set; }

        [DefaultValue(false)]
        public InArgument<bool> ExportEmailTrackingSettings { get; set; }

        [DefaultValue(false)]
        public InArgument<bool> ExportGeneralSettings { get; set; }

        [DefaultValue(false)]
        public InArgument<bool> ExportIsvConfig { get; set; }

        [DefaultValue(false)]
        public InArgument<bool> ExportMarketingSettings { get; set; }

        [DefaultValue(false)]
        public InArgument<bool> ExportOutlookSynchronizationSettings { get; set; }

        [DefaultValue(false)]
        public InArgument<bool> ExportRelationshipRoles { get; set; }

        public OutArgument<string> OutputFile { get; set; }

        #endregion

        #region Execute

        protected override void Execute(CodeActivityContext context)
        {
            CrmConnection connection = CrmConnection.Parse(CrmConnectionString.Get(context));

            using (OrganizationService orgService = new OrganizationService(connection))
            {
                Common.ExportSolution export = new Common.ExportSolution(orgService);

                OutputFile.Set(context, export.Export(Managed.Get(context),
                                                        OutputFolder.Get(context),
                                                        UniqueSolutionName.Get(context),
                                                        ExportAutoNumberingSettings.Get(context),
                                                        ExportCalendarSettings.Get(context),
                                                        ExportCustomizationSettings.Get(context),
                                                        ExportEmailTrackingSettings.Get(context),
                                                        ExportGeneralSettings.Get(context),
                                                        ExportIsvConfig.Get(context),
                                                        ExportMarketingSettings.Get(context),
                                                        ExportOutlookSynchronizationSettings.Get(context),
                                                        ExportRelationshipRoles.Get(context)));

            }
        }

        #endregion
    }
}
