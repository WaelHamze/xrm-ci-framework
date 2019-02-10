using Microsoft.Xrm.Sdk;
using System.Management.Automation;
using Xrm.Framework.CI.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public abstract class XrmCommandBase : CommandBase
    {
        protected virtual IOrganizationService OrganizationService { get; private set; }

        /// <summary>
        /// <para type="description">The connectionstring to the crm organization (see https://msdn.microsoft.com/en-us/library/mt608573.aspx ).</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string ConnectionString { get; set; }

        /// <summary>
        /// <para type="description">Timeout in seconds</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int Timeout { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            XrmConnectionManager xrmConnection = new XrmConnectionManager(
                Logger);

            OrganizationService = xrmConnection.Connect(
                ConnectionString,
                Timeout);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}