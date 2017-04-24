using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Build.Client;
using System.Activities;
using Microsoft.Xrm.Client.Services;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;

namespace Xrm.Framework.CI.TeamFoundation.Activities
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class PublishCustomizations : CodeActivity
    {
        #region Arguments

        [RequiredArgument]
        public InArgument<string> CrmConnectionString { get; set; }

        #endregion

        #region Execute

        protected override void Execute(CodeActivityContext context)
        {
            CrmConnection connection = CrmConnection.Parse(CrmConnectionString.Get(context));

            using (OrganizationService orgService = new OrganizationService(connection))
            {
                PublishAllXmlRequest req = new PublishAllXmlRequest();
                orgService.Execute(req);

            }
        }

        #endregion
    }
}
