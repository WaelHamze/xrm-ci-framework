using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public class Step
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string MessageName { get; set; }

        public string Description { get; set; }

        public string CustomConfiguration { get; set; }

        public string FilteringAttributes { get; set; }

        public string ImpersonatingUserFullname { get; set; }

        public string Mode { get; set; }

        public string PrimaryEntityName { get; set; }

        public int? Rank { get; set; }

        public string Stage { get; set; }

        public string SupportedDeployment { get; set; }

        public List<Image> Images { get; set; }
    }
}
