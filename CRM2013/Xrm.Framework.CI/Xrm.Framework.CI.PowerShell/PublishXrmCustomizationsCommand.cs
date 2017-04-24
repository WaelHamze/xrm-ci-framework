using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;

namespace Xrm.Framework.CI.PowerShell
{
    [Cmdlet(VerbsData.Publish, "XrmCustomizations")]
    public class PublishXrmCustomizationsCommand : Cmdlet
    {
        #region Parameters

        [Parameter(Mandatory = true)]
        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }
        private string connectionString;

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Publishing Customizations"));

            CrmConnection connection = CrmConnection.Parse(connectionString);

            using (OrganizationService service = new OrganizationService(connection))
            {
                PublishAllXmlRequest req = new PublishAllXmlRequest();
                service.Execute(req);

                base.WriteVerbose(string.Format("Publish Customizations Completed"));
            }
        }

        #endregion
    }
}
