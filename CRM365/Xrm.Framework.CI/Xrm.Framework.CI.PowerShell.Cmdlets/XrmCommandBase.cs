using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System.Configuration;
using System.Threading;
using System;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public abstract class XrmCommandBase : Cmdlet
    {
        protected IOrganizationService OrganizationService;
        protected CrmServiceClient ServiceClient;
        private int DefaultTime = 120;
        private TimeSpan ConnectPolingInterval = TimeSpan.FromSeconds(15);
        private int ConnectRetryCount = 3;

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

            for (int i = 1; i <= ConnectRetryCount; i++)
            {
                WriteVerbose(string.Format("Connecting to CRM [attempt {0}]", i));
                ServiceClient = new CrmServiceClient(ConnectionString);

                if (ServiceClient != null && ServiceClient.IsReady)
                {
                    if (Timeout == 0)
                    {
                        ServiceClient.OrganizationServiceProxy.Timeout = new System.TimeSpan(0, 0, DefaultTime);
                    }
                    else
                    {
                        ServiceClient.OrganizationServiceProxy.Timeout = new System.TimeSpan(0, 0, Timeout);
                    }
                    OrganizationService = ServiceClient;
                    return;
                }
                else
                {
                    if (i != ConnectRetryCount)
                        Thread.Sleep(ConnectPolingInterval);
                }
            }

            throw new Exception(string.Format("Couldn't connect to CRM instance after {0} attempts: {1}", ConnectRetryCount, ServiceClient?.LastCrmError));
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();

            if (ServiceClient != null)
                ServiceClient.Dispose();
        }
    }
}