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
    [Cmdlet(VerbsCommon.Add, "XrmEntity")]
    public class AddXrmEntityCommand : Cmdlet
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
        public Entity EntityObject
        {
            get { return entityObject; }
            set { entityObject = value; }
        }
        private Entity entityObject;

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Creating Entity: {0}", EntityObject));

            CrmConnection connection = CrmConnection.Parse(connectionString);

            using (OrganizationService service = new OrganizationService(connection))
            {
                Guid id = service.Create(EntityObject);

                base.WriteObject(id);

                base.WriteVerbose(string.Format("Entity Created: {0}", EntityObject));
            }
        }

        #endregion
    }
}
