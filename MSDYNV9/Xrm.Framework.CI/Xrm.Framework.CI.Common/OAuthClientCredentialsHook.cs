using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Xrm.Tooling.Connector;

namespace Xrm.Framework.CI.Common
{

    public class OAuthClientCredentialsHook : IOverrideAuthHookWrapper
    {
        private readonly ConcurrentDictionary<Uri, AuthenticationParameters> authenticationParameters =
            new ConcurrentDictionary<Uri, AuthenticationParameters>();

        private readonly Dictionary<string, AuthenticationResult> authenticationResults =
            new Dictionary<string, AuthenticationResult>();

        private readonly string clientSecret;
        private readonly string clientId;

        public OAuthClientCredentialsHook(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        public string GetAuthToken(Uri connectedUri)
        {
            if (!authenticationResults.ContainsKey(connectedUri.Host) ||
                authenticationResults[connectedUri.Host].ExpiresOn < DateTime.UtcNow.AddSeconds(30))
            {
                var parameters = authenticationParameters.GetOrAdd(GetWebApiUri(connectedUri), GetAuthenticationParameters);
                authenticationResults[connectedUri.Host] = GetAccessToken(parameters);
            }

            return authenticationResults[connectedUri.Host].AccessToken;
        }

        private AuthenticationResult GetAccessToken(AuthenticationParameters authParameters)
        {
            var authContext = new AuthenticationContext(authParameters.Authority, false);
            return authContext.AcquireTokenAsync(authParameters.Resource, new ClientCredential(clientId, clientSecret)).Result;
        }

        private Uri GetWebApiUri(Uri uri) =>
            new Uri(uri.GetLeftPart(UriPartial.Authority) + "/api/data/v9.1/");

        private AuthenticationParameters GetAuthenticationParameters(Uri webApiUri) =>
            AuthenticationParameters.CreateFromResourceUrlAsync(webApiUri).Result;
    }
}
