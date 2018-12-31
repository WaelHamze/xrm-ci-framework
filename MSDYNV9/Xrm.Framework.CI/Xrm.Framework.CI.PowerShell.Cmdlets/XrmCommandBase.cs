using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Management.Automation;
using System.Net;
using System.Threading;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Logging;
using Xrm.Framework.CI.PowerShell.Cmdlets.Logging;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public abstract class XrmCommandBase : Cmdlet
    {
        protected CrmServiceClient ServiceClient;
        private int DefaultTime = 120;
        private TimeSpan ConnectPolingInterval = TimeSpan.FromSeconds(15);
        private int ConnectRetryCount = 3;

        protected virtual IOrganizationService OrganizationService { get; private set; }
        protected virtual ILogger Logger { get; set; }

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

            Logger = new PSLogger(this);

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