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
            string name = "CrmConnection";
            //string connectionString = GetConnectionString(name);
            string connectionString = "AuthType=Office365;Username=admin@ultradynamics.co.uk;Password=MSDYN365!!;Url=https://ultradynamicsprod.crm11.dynamics.com";

            return  new CrmServiceClient(connectionString);
        }

        private string GetConnectionString(string name)
        {
            string value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
            if (string.IsNullOrEmpty(value))
                value = ConfigurationManager.ConnectionStrings[name].ConnectionString;
            if (string.IsNullOrEmpty(value))
                throw new Exception(name);
            return value;
        }

        #endregion
    }
}
