using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Xrm.Tooling.Connector;

namespace Xrm.Framework.CI.Common
{
    internal class OAuthCredentialsHook : IOverrideAuthHookWrapper
    {
        private readonly string clientSecret;
        private readonly string clientId;

        public OAuthCredentialsHook(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        public string GetAuthToken(Uri connectedUri)
        {
            var parameters = AuthenticationParameters.CreateFromResourceUrlAsync(connectedUri).Result;
            var authContext = new AuthenticationContext(parameters.Authority, false);
            return authContext.AcquireTokenAsync(parameters.Resource, new ClientCredential(this.clientId, this.clientSecret)).Result.AccessToken;
        }
    }
}