using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public class EnvironmentVariablesManager : XrmBase
    {
        #region Variables


        #endregion

        #region Constructors

        public EnvironmentVariablesManager(ILogger logger,
            IOrganizationService organizationService)
            : base(logger, organizationService)
        {

        }

        #endregion

        #region Methods

        public string GetValue(string name)
        {
            EnvironmentVariableValue env = GetValueRecord(name);

            if (env != null)
            {
                return env.Value;
            }
            return null;
        }

        public void SetValue(string name, string value)
        {
            EnvironmentVariableValue current = GetValueRecord(name);

            if (current != null)
            {
                current.Value = value;

                EnvironmentVariableValue update = new EnvironmentVariableValue();
                update.Id = current.Id;
                update.Value = value;
                OrganizationService.Update(update);
            }
            else
            {
                EnvironmentVariableDefinition definition = GetDefinitionRecord(name);

                if (definition is null)
                {
                    throw new Exception($"Definition with SchemaName = {name} was not found");
                }

                EnvironmentVariableValue create = new EnvironmentVariableValue();
                create.Value = value;
                create.EnvironmentVariableDefinitionId = definition.ToEntityReference();
                Guid Id = OrganizationService.Create(create);
            }
        }

        public void DeleteValue(string name)
        {
            EnvironmentVariableValue current = GetValueRecord(name);

            if (current != null)
            {
                OrganizationService.Delete(current.LogicalName, current.Id);
            }
        }

        private EnvironmentVariableDefinition GetDefinitionRecord(string name)
        {
            using (var context = new CIContext(OrganizationService))
            {
                var query = from def in context.EnvironmentVariableDefinitionSet
                            where def.SchemaName == name
                            select def;

                var definitions = query.ToList<EnvironmentVariableDefinition>();

                if (definitions.Count == 0)
                {
                    return null;
                }
                else if (definitions.Count == 1)
                {
                    return definitions[0];
                }
                else
                {
                    throw new Exception($"Mutiple definitions with SchemaName = {name}");
                }
            }
        }

        private EnvironmentVariableValue GetValueRecord(string name)
        {
            using (var context = new CIContext(OrganizationService))
            {
                var query = from env in context.EnvironmentVariableValueSet
                            join def in context.EnvironmentVariableDefinitionSet
                            on env.EnvironmentVariableDefinitionId.Id equals def.Id
                            where def.SchemaName == name
                            select env;

                var values = query.ToList<EnvironmentVariableValue>();

                if (values.Count == 0)
                {
                    return null;
                }
                else if (values.Count == 1)
                {
                    return values[0];
                }
                else
                {
                    throw new Exception($"Mutiple values found for definition with SchemaName = {name}");
                }
            }
        }

        #endregion
    }

    //public Component
}
