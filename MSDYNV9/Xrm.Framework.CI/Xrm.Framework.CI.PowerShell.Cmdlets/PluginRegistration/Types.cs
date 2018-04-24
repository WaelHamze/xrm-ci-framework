using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public class Type
    {
        public string Description { get; set; }

        public string Name { get; set; }

        public string FriendlyName { get; set; }

        public string TypeName { get; set; }

        public List<Step> Steps { get; set; }

        public string WorkflowActivityGroupName { get; set; }
    }
}