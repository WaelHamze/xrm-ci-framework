using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xrm.Framework.CI.Common.IntegrationTests
{
    public class XrmConnectionManager
    {
        #region Constructors
        
        public XrmConnectionManager()
        {

        }

        #endregion

        #region Methods

        public IOrganizationService CreateConnection()
        {
            string name = "CrmConn";

            string connectionString = GetConnectionString(name);

            return  new CrmServiceClient(connectionString);
        }

        private string GetConnectionString(string name)
        {
            string value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
            if (string.IsNullOrEmpty(value))
                value = ConfigurationManager.ConnectionStrings[name].ConnectionString;
            if (string.IsNullOrEmpty(value))
                throw new Exception($"connection with {name} was not found");
            return value;
        }

        #endregion
    }
}
