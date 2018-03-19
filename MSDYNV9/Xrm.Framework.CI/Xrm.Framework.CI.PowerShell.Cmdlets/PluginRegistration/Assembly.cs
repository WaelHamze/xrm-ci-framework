using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public class Assembly
    {
        public string Name { get; set; }

        public string IsolationMode { get; set; }

        public string SourceType { get; set; }

        public List<Type> PluginTypes { get; set; }
    }
}
