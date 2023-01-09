using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using System.Configuration;
using System.Threading;
using System;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public abstract class XrmCommandBase : Cmdlet
    {
        protected IOrganizationService OrganizationService;
        protected ServiceClient ServiceClient;
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
                ServiceClient = new ServiceClient(ConnectionString);

                if (ServiceClient != null && ServiceClient.IsReady)
                {
                    if (Timeout == 0)
                    {
                        ServiceClient.MaxConnectionTimeout = new System.TimeSpan(0, 0, DefaultTime);
                    }
                    else
                    {
                        ServiceClient.MaxConnectionTimeout = new System.TimeSpan(0, 0, Timeout);
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

            throw new Exception(string.Format("Couldn't connect to CRM instance after {0} attempts: {1}", ConnectRetryCount, ServiceClient?.LastError));
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();

            if (ServiceClient != null)
                ServiceClient.Dispose();
        }
    }
}