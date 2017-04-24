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

namespace Xrm.Framework.CI.PowerShell
{
    [Cmdlet(VerbsCommon.Remove, "XrmEntity")]
    public class RemoveXrmEntityCommand : Cmdlet
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
        public string EntityName
        {
            get { return entityName; }
            set { entityName = value; }
        }
        private string entityName;

        [Parameter(Mandatory = true)]
        public Guid Id
        {
            get { return id; }
            set { id = value; }
        }
        private Guid id;

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Deleting Entity: {0} {1}", EntityName, Id));

            CrmConnection connection = CrmConnection.Parse(connectionString);

            using (OrganizationService service = new OrganizationService(connection))
            {
                service.Delete(EntityName, Id);

                base.WriteVerbose(string.Format("Entity Deleted: {0} {1}", EntityName, Id));
            }
        }

        #endregion
    }
}
