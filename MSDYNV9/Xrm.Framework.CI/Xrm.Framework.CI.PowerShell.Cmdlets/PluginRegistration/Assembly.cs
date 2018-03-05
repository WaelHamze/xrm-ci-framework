using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public class Assembly
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int IsolationMode { get; set; }

        public int SourceType { get; set; }

        public List<Type> PluginTypes { get; set; }
    }
}
