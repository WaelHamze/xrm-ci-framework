using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace Xrm.Framework.CI.Common
{
    internal class CrmConnectionString
    {
        public Uri ServiceUri
        {
            get;
            internal set;
        }

        public AuthenticationType AuthenticationType
        {
            get;
            internal set;
        }

        public string ClientId
        {
            get;
            internal set;
        }

        public string Password
        {
            get;
            set;
        }

        public string UserId
        {
            get;
            internal set;
        }

        public bool UseUniqueConnectionInstance
        {
            get;
            internal set;
        }

        private CrmConnectionString(string serviceUri, string userName, string password, string authType, string requireNewInstance, string clientId)
        {
            this.ServiceUri = this.GetValidUri(serviceUri);
            this.UserId = (!string.IsNullOrWhiteSpace(userName) ? userName : string.Empty);
            this.Password = (!string.IsNullOrWhiteSpace(password) ? password : string.Empty);
            this.ClientId = (!string.IsNullOrWhiteSpace(clientId) ? clientId : string.Empty);

            if (!Enum.TryParse<AuthenticationType>(authType, out AuthenticationType authenticationType))
            {
                this.AuthenticationType = AuthenticationType.AD;
            }
            else
            {
                this.AuthenticationType = authenticationType;
            }

            bool.TryParse(requireNewInstance, out bool useUniqueConnectionInstance);
            this.UseUniqueConnectionInstance = useUniqueConnectionInstance;
        }

        public static CrmConnectionString Parse(string connectionString)
        {
            var dictionary = connectionString.ToDictionary();
            var serviceUri = dictionary.FirstNotNullOrEmpty<string>(new string[] { "ServiceUri", "Service Uri", "Url", "Server" });
            var userName = dictionary.FirstNotNullOrEmpty<string>(new string[] { "UserName", "User Name", "UserId", "User Id" });
            var password = dictionary.FirstNotNullOrEmpty<string>(new string[] { "Password" });
            var authType = dictionary.FirstNotNullOrEmpty<string>(new string[] { "AuthType", "AuthenticationType" });
            var requireNewInstance = dictionary.FirstNotNullOrEmpty<string>(new string[] { "RequireNewInstance" });
            var clientId = dictionary.FirstNotNullOrEmpty<string>(new string[] { "ClientId", "AppId", "ApplicationId" });


            return new CrmConnectionString(serviceUri, userName, password, authType, requireNewInstance, clientId);
        }

        private Uri GetValidUri(string uriSource)
        { 
            Uri validUriResult;
            if (Uri.TryCreate(uriSource, UriKind.Absolute, out validUriResult))
            {
                return validUriResult;
            }

            return null;
        }
    }
}
