using System;
using System.Collections.Generic;
using System.Linq;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.Common
{
    public class Assembly
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public PluginAssembly_IsolationMode? IsolationMode { get; set; }

        public PluginAssembly_SourceType? SourceType { get; set; }

        public string Version { get; set; }

        public List<Type> PluginTypes { get; set; }

        public Assembly()
        {
            PluginTypes = new List<Type>();
        }

        public static Assembly operator +(Assembly b, Assembly c)
        {
            var assembly = new Assembly
            {
                Id = c.Id,
                IsolationMode = b.IsolationMode,
                Name = b.Name,
                PluginTypes = new List<Type>(),
                SourceType = b.SourceType
            };

            if (b.PluginTypes == null) return assembly;

            foreach (var pluginType in b.PluginTypes)
            {
                var original = b.PluginTypes.First(x => x.SameAsRegistered(pluginType));
                var corresponding = c.PluginTypes?.FirstOrDefault(x => x.SameAsRegistered(pluginType));
                if (corresponding != null)
                {
                    original += corresponding;
                }
                assembly.PluginTypes.Add(original);
            }

            return assembly;
        }
    }

    public static class AssemblyExtensions
    {
        public static bool SameAsRegistered(this Assembly original, Assembly compare)
        {
            return original != null && (compare != null && original.Name == compare.Name);
        }
    }
}
