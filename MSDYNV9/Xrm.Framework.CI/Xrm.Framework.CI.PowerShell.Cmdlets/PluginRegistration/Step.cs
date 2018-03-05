using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public class Step
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string MessageName { get; set; }

        public string Description { get; set; }

        public string CustomConfiguration { get; set; }

        public string FilteringAttributes { get; set; }

        public Guid ImpersonatingUserId { get; set; }

        public int Mode { get; set; }

        public string PrimaryEntityName { get; set; }

        public int? Rank { get; set; }

        public int Stage { get; set; }

        public int SupportedDeployment { get; set; }

        public List<Image> Images { get; set; }
    }
}
