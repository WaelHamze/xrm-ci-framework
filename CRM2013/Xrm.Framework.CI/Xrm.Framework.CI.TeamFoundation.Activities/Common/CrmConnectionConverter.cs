using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Client;

namespace Xrm.Framework.CI.Common
{
    public class CrmConnectionConverter
    {
        //public static string ConvertToConnectionString()
        //{

        //}

        public static CrmConnectionAttributes ConvertFromConnectionString(string connectionString)
        {
            CrmConnectionAttributes conAttributes = new CrmConnectionAttributes();

            CrmConnection connection = CrmConnection.Parse(connectionString);
            Uri serviceUri = connection.ServiceUri;

            conAttributes.Protocol = serviceUri.Scheme;
            conAttributes.Port = serviceUri.Port.ToString();

            if (connection.ClientCredentials != null && connection.ClientCredentials.Windows != null && connection.ClientCredentials.Windows.ClientCredential != null)
            {
                conAttributes.Domain = connection.ClientCredentials.Windows.ClientCredential.Domain;
                conAttributes.UserName = connection.ClientCredentials.Windows.ClientCredential.UserName;
                conAttributes.Password = connection.ClientCredentials.Windows.ClientCredential.Password;
            }
            else if (connection.ClientCredentials != null && connection.ClientCredentials.UserName != null)
            {
                conAttributes.UserName = connection.ClientCredentials.UserName.UserName;
                conAttributes.Password = connection.ClientCredentials.UserName.Password;
            }

            return conAttributes;
        }
    }
}
