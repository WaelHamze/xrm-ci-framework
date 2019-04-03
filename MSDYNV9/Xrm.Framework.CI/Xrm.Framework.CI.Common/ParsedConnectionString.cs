using Microsoft.Xrm.Tooling.Connector;
using System;

namespace Xrm.Framework.CI.Common
{
    internal class ParsedConnectionString
    {
        public InternalAuthenticationType AuthType { get; private set; }
        public bool RequireNewInstance { get; private set; }
        public string ClientSecret { get; private set; }
        public string ClientId { get; private set; }
        public Uri Server { get; private set; }

        public ParsedConnectionString(string connectionString)
        {
            RequireNewInstance = GetRequireNewInstance(connectionString);
            ClientSecret = GetClientSecret(connectionString);
            ClientId = GetClientId(connectionString);
            AuthType = GetAuthType(connectionString);
            var server = GetServerString(connectionString);

            if (!Uri.IsWellFormedUriString(server, UriKind.Absolute))
            {
                throw new ArgumentException(nameof(Server));
            }

            Server = new Uri(server);

            if (AuthType != InternalAuthenticationType.S2S)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(ClientId))
            {
                throw new ArgumentException(nameof(ClientId));
            }

            if (string.IsNullOrWhiteSpace(ClientSecret))
            {
                throw new ArgumentException(nameof(ClientSecret));
            }
        }

        private static InternalAuthenticationType GetAuthType(string connectionString) =>
            Enum.TryParse(connectionString
                .ToDictionary()
                .FirstNotNullOrEmpty(nameof(AuthType)), true, out InternalAuthenticationType result) ? result : InternalAuthenticationType.AD;

        private static string GetClientId(string connectionString) =>
            connectionString
                .ToDictionary()
                .FirstNotNullOrEmpty(nameof(ClientId));

        private static string GetClientSecret(string connectionString) =>
            connectionString
                .ToDictionary()
                .FirstNotNullOrEmpty(nameof(ClientSecret));

        private static bool GetRequireNewInstance(string connectionString) =>
            bool.TryParse(connectionString
                .ToDictionary()
                .FirstNotNullOrEmpty(nameof(RequireNewInstance)), out bool result) && result;

        private static string GetServerString(string connectionString) =>
            connectionString
                .ToDictionary()
                .FirstNotNullOrEmpty("ServiceUri", "Service Uri", "Url", nameof(Server));

        private static Uri GetServer(string connectionString) =>
            new Uri(GetServerString(connectionString));
    }

    internal enum InternalAuthenticationType
    {
        InvalidConnection = -1,
        AD = 0,
        Live = 1,
        IFD = 2,
        Claims = 3,
        Office365 = 4,
        OAuth = 5,
        Certificate = 6,
        ExternalTokenManagement = 99, // 0x00000063
        S2S
    }
}
