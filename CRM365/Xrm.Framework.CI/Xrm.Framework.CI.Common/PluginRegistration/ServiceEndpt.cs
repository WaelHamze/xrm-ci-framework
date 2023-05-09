using System;
using System.Collections.Generic;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.Common
{
    public class ServiceEndpt
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public string NamespaceAddress { get; set; }

        public ServiceEndpoint_Contract? Contract { get; set; }

        public string Path { get; set; }

        public ServiceEndpoint_MessageFormat? MessageFormat { get; set; }

        public ServiceEndpoint_AuthType? AuthType { get; set; }

        public string SASKeyName { get; set; }

        public string SASKey { get; set; }

        public string SASToken { get; set; }

        public ServiceEndpoint_UserClaim? UserClaim { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public string AuthValue { get; set; }

        public List<Step> Steps { get; set; }
    }
}
