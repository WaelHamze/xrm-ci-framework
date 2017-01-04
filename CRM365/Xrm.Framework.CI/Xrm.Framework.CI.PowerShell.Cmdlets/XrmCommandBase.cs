using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System.Configuration;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public abstract class XrmCommandBase : Cmdlet
    {
        protected IOrganizationService OrganizationService;
        protected CrmServiceClient ServiceClient;

        /// <summary>
        /// <para type="description">The connectionstring to the crm organization (see https://msdn.microsoft.com/en-us/library/mt608573.aspx ).</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string ConnectionString { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            ServiceClient = new CrmServiceClient(ConnectionString);
            OrganizationService = ServiceClient;
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();

            if (ServiceClient != null)
                ServiceClient.Dispose();
        }
    }
}