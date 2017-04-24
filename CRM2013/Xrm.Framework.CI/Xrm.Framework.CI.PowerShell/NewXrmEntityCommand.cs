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
    [Cmdlet(VerbsCommon.New, "XrmEntity")]
    public class NewXrmEntityCommand : Cmdlet
    {
        #region Parameters

        [Parameter(Mandatory = true)]
        public string EntityName
        {
            get { return entityName; }
            set { entityName = value; }
        }
        private string entityName;

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("New Entity: {0}", EntityName));

            WriteObject(new Entity(EntityName));
        }

        #endregion
    }
}
