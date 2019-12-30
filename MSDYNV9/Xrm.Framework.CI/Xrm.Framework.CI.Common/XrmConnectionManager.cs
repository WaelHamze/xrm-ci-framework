﻿using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public class XrmConnectionManager
    {
        #region Variables

        private int DefaultTime = 120;
        private TimeSpan ConnectPolingInterval = TimeSpan.FromSeconds(15);
        private int ConnectRetryCount = 3;

        #endregion

        #region Properties

        protected ILogger Logger
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public XrmConnectionManager(ILogger logger)
        {
            Logger = logger;
        }

        #endregion

        #region Methods

        public IOrganizationService Connect(
            string connectionString,
            int timeout)
        {
            PrintSdkVersion();
            SetSecurityProtocol();
            return ConnectToCRM(connectionString, timeout);
        }

        private void SetSecurityProtocol()
        {
            Logger.LogVerbose("Current Security Protocol: {0}", ServicePointManager.SecurityProtocol);

            if (!ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls11))
            {
                ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol ^ SecurityProtocolType.Tls11;
            }
            if (!ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls12))
            {
                ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol ^ SecurityProtocolType.Tls12;
            }

            Logger.LogVerbose("Modified Security Protocol: {0}", ServicePointManager.SecurityProtocol);
        }

        private void PrintSdkVersion()
        {
            string sdkAssembly = typeof(Microsoft.Xrm.Sdk.Entity).Assembly.Location;
            string connectorAssembley = typeof(Microsoft.Xrm.Tooling.Connector.CrmServiceClient).Assembly.Location;

            FileInfo sdkInfo = new FileInfo(sdkAssembly);
            FileInfo connectorInfo = new FileInfo(connectorAssembley);

            string sdkVersion = FileUtilities.GetFileVersion(sdkInfo.FullName);
            string toolingVersion = FileUtilities.GetFileVersion(connectorInfo.FullName);

            Logger.LogVerbose("Microsoft.Xrm.Sdk.dll - Version : {0}", sdkVersion);
            Logger.LogVerbose("Microsoft.Xrm.Tooling.Connector.dll - Version : {0}", toolingVersion);
        }

        private IOrganizationService ConnectToCRM(string connectionString, int timeout)
        {
            CrmServiceClient.MaxConnectionTimeout = TimeSpan.FromMinutes(timeout == 0 ? DefaultTime : timeout);

            CrmServiceClient serviceClient = null;
            for (int i = 1; i <= ConnectRetryCount; i++)
            {
                Logger.LogVerbose("Connecting to CRM [attempt {0}]", i);
                serviceClient = new CrmServiceClient(connectionString);

                if (serviceClient != null && serviceClient.IsReady)
                {
                    if (serviceClient.OrganizationServiceProxy != null)
                    {
                        Logger.LogVerbose("Connection to CRM Established using OrganizationServiceProxy");
                    }
                    else
                    {
                        Logger.LogVerbose("Connection to CRM Established using OrganizationWebProxyClient");
                    }

                    return serviceClient;
                }
                else
                {
                    Logger.LogWarning(serviceClient.LastCrmError);
                    if (serviceClient.LastCrmException != null)
                    {
                        Logger.LogWarning(serviceClient.LastCrmException.Message);
                    }
                    if (i != ConnectRetryCount)
                        Thread.Sleep(ConnectPolingInterval);
                }
            }

            throw new Exception(string.Format("Couldn't connect to CRM instance after {0} attempts: {1}", ConnectRetryCount, serviceClient?.LastCrmError));
        }

        #endregion
    }
}
