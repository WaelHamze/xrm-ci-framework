using System;
using System.Collections.Generic;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public class Assembly
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public PluginAssembly_IsolationMode? IsolationMode { get; set; }

        public PluginAssembly_SourceType? SourceType { get; set; }

        public List<Type> PluginTypes { get; set; }

        public Assembly()
        {
            PluginTypes = new List<Type>();
        }
    }
}
