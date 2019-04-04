﻿using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Net;
using System.Threading;

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

        private IOrganizationService ConnectToCRM(string connectionString, int timeout)
        {
            var parsedConnectionString = new ParsedConnectionString(connectionString);
            Logger.LogVerbose("ConnectionString info - AuthType = {0}, Server = {1}", parsedConnectionString.AuthType, parsedConnectionString.Server);
            if (parsedConnectionString.AuthType == InternalAuthenticationType.S2S)
            {
                CrmServiceClient.AuthOverrideHook = new OAuthClientCredentialsHook(parsedConnectionString.ClientId, parsedConnectionString.ClientSecret);
            }

            CrmServiceClient.MaxConnectionTimeout = TimeSpan.FromMinutes(timeout == 0 ? DefaultTime : timeout);
            CrmServiceClient serviceClient = null;
            for (int i = 1; i <= ConnectRetryCount; i++)
            {
                Logger.LogVerbose("Connecting to CRM [attempt {0}]", i);
                if (parsedConnectionString.AuthType == InternalAuthenticationType.S2S)
                {
                    serviceClient = new CrmServiceClient(parsedConnectionString.Server, parsedConnectionString.RequireNewInstance);
                }
                else
                {
                    serviceClient = new CrmServiceClient(connectionString);
                }

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
