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
    [Cmdlet(VerbsCommon.Select, "WhoAmI")]
    public class SelectXrmWhoAmICommand : Cmdlet
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

            base.WriteVerbose(string.Format("Invoking Organization Service"));

            CrmConnection connection = CrmConnection.Parse(connectionString);

            using (OrganizationService service = new OrganizationService(connection))
            {
                WhoAmIResponse response = (service.Execute(new WhoAmIRequest()) as WhoAmIResponse);

                base.WriteObject(response);

                base.WriteVerbose(string.Format("OrganizationId: {0}", response.OrganizationId));
                base.WriteVerbose(string.Format("BusinessUnitId: {0}", response.BusinessUnitId));
                base.WriteVerbose(string.Format("UserId: {0}", response.UserId));
            }
        }

        #endregion
    }
}
