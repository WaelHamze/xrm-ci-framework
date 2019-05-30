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
    public class TestConnectionManager
    {
        #region Constructors
        
        public TestConnectionManager()
        {

        }

        #endregion

        #region Methods

        public IOrganizationService CreateConnection()
        {
            string name = "CrmConnection";

            string connectionString = GetConnectionString(name);

            XrmConnectionManager con
                = new XrmConnectionManager(new Logging.TestLogger());

            return con.Connect(connectionString, 0);
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
