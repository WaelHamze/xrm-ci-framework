using System;
using System.Collections.Generic;

namespace Xrm.Framework.CI.Common.PluginRegistration
{
    public class Package
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public List<Assembly> Assemblies { get; set; }

        public Package()
        {
            Assemblies = new List<Assembly>();
        }
    }
}