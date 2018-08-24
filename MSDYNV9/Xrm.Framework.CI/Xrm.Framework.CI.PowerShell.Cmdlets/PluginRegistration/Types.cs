using System;
using System.Collections.Generic;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public class Type
    {
        public Guid? Id { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string FriendlyName { get; set; }

        public string TypeName { get; set; }

        public List<Step> Steps { get; set; }

        public string WorkflowActivityGroupName { get; set; }
    }
}