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
            Logger.LogVerbose("Getting EnvironmentVariableValue for EnvironmentVariableDefinition with SchemeName {0}", name);

            EnvironmentVariableValue env = GetValueRecord(name);

            if (env != null)
            {
                Logger.LogInformation("Returning EnvironmentVariableValue record found with Id = {0}", env.Id);
                return env.Value;
            }
            else
            {
                Logger.LogInformation("No EnvironmentVariableValue record found for EnvironmentVariableDefinition with SchemeName {0}", name);
                return null;
            }
        }

        public void SetValue(string name, string value)
        {
            Logger.LogVerbose("Setting EnvironmentVariableValue for EnvironmentVariableDefinition with SchemeName {0}", name);

            EnvironmentVariableValue current = GetValueRecord(name);

            if (current != null)
            {
                if (current.Value != value)
                {
                    current.Value = value;

                    EnvironmentVariableValue update = new EnvironmentVariableValue();
                    update.Id = current.Id;
                    update.Value = value;
                    OrganizationService.Update(update);

                    Logger.LogInformation("Updated EnvironmentVariableValue Id ={0} for EnvironmentVariableDefinition with SchemeName {1}", current.Id, name);
                }
                else
                {
                    Logger.LogInformation("Skipped Update EnvironmentVariableValue Id ={0} for EnvironmentVariableDefinition with SchemeName {1} as values are same", current.Id, name);
                }
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

                Logger.LogInformation("Created EnvironmentVariableValue Id ={0} for EnvironmentVariableDefinition with SchemeName {1}", Id, name);
            }
        }

        public void DeleteValue(string name)
        {
            Logger.LogVerbose("Deleting EnvironmentVariableValue for EnvironmentVariableDefinition with SchemeName {0}", name);

            EnvironmentVariableValue current = GetValueRecord(name);

            if (current != null)
            {
                Logger.LogVerbose("Deleting EnvironmentVariableValue with Id = {0}", current.Id);
                OrganizationService.Delete(current.LogicalName, current.Id);
                Logger.LogInformation("Deleted EnvironmentVariableValue with Id = {0}", current.Id);
            }
            else
            {
                Logger.LogInformation("Skipping Delete as no EnvironmentVariableValue found  for EnvironmentVariableDefinition with SchemeName {0}", name);
            }
        }

        private EnvironmentVariableDefinition GetDefinitionRecord(string name)
        {
            Logger.LogVerbose("Retrieving EnvironmentVariableDefinition with SchemeName {0}", name);

            using (var context = new CIContext(OrganizationService))
            {
                var query = from def in context.EnvironmentVariableDefinitionSet
                            where def.SchemaName == name
                            select def;

                var definitions = query.ToList<EnvironmentVariableDefinition>();

                if (definitions.Count == 0)
                {
                    Logger.LogVerbose("No EnvironmentVariableDefinition records found");
                    return null;
                }
                else if (definitions.Count == 1)
                {
                    Logger.LogVerbose("EnvironmentVariableDefinition record found with Id = {0}", definitions[0].Id);
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
            Logger.LogVerbose("Retrieving EnvironmentVariableValue for EnvironmentVariableDefinition with SchemeName {0}", name);

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
                    Logger.LogVerbose("No EnvironmentVariableValue records found");

                    return null;
                }
                else if (values.Count == 1)
                {
                    Logger.LogVerbose("EnvironmentVariableValue record found with Id = {0}", values[0].Id);
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
