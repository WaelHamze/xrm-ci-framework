using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Client.Services;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.Xrm.Client;

namespace Xrm.Framework.CI.TeamFoundation.Activities
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public sealed class UpdateSolution : CodeActivity
    {
        #region Arguments

        [RequiredArgument]
        public InArgument<string> CrmConnectionString { get; set; }

        [RequiredArgument]
        public InArgument<string> UniqueSolutionName { get; set; }

        [RequiredArgument]
        public InArgument<string> Version { get; set; }

        #endregion
        
        #region Execute

        protected override void Execute(CodeActivityContext context)
        {
            CrmConnection connection = CrmConnection.Parse(CrmConnectionString.Get(context));

            using (OrganizationService orgService = new OrganizationService(connection))
            {
                Common.UpdateSolution update = new Common.UpdateSolution(orgService);

                update.Update(UniqueSolutionName.Get(context), Version.Get(context));
            }
        }

        #endregion
    }
}
