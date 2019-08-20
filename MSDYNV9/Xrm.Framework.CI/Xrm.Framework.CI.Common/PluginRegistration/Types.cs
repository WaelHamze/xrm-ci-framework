using System;
using System.Collections.Generic;
using System.Linq;

namespace Xrm.Framework.CI.Common
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

        public static Type operator +(Type b, Type c)
        {
            var pluginType = new Type
            {
                Id = c.Id,
                Description = b.Description,
                FriendlyName = b.FriendlyName,
                TypeName = b.TypeName,
                Name = b.Name,
                Steps = new List<Step>(),
                WorkflowActivityGroupName = b.WorkflowActivityGroupName
            };

            if (b.Steps == null) return pluginType;

            foreach (var pluginStep in b.Steps)
            {
                var original = b.Steps.First(x => x.SameAsRegistered(pluginStep));
                var corresponding = c.Steps?.FirstOrDefault(x => x.SameAsRegistered(pluginStep));
                if (corresponding != null)
                {
                    original += corresponding;
                }

                pluginType.Steps.Add(original);
            }

            return pluginType;
        }
    }

    public static class TypeExtensions
    {
        public static bool SameAsRegistered(this Type original, Type compare)
        {
            return original != null
                   && (compare != null
                       && original.Name == compare.Name
                       && original.TypeName == compare.TypeName
                       && original.WorkflowActivityGroupName == compare.WorkflowActivityGroupName);
        }
    }
}