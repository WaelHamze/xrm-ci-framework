using System.Management.Automation;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;

namespace Xrm.Framework.CI.PowerShell.Command
{
    public abstract class XrmCommandBase : Cmdlet
    {
        protected IOrganizationService OrganizationService;

        /// <summary>
        /// <para type="description">The connectionstring to the crm organization (see http://msdn.microsoft.com/en-us/library/gg695810.aspx ).</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string ConnectionString { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            CrmConnection connection = CrmConnection.Parse(ConnectionString);
            OrganizationService = new OrganizationService(connection);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();

            var typedService = OrganizationService as OrganizationService;
            if (typedService != null)
                typedService.Dispose();
        }
    }
}