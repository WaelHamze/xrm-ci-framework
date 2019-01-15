using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Remove a workflow.</para>
    /// <para type="description">The Remove-XrmWorkflow cmdlet removes an existing workflow or workflows in CRM.
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Remove-XrmWorkflow -Name $workflowNamePattern</code>
    ///   <para>Workflow Name Pattern to Remove</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "XrmWorkflow")]
    public class RemoveXrmWorkflow : XrmCommandBase
    {
        #region Parameters
        /// <summary>
        /// <para type="description">The assembly name. e.g. Contoso.Plugin.dll</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The assembly name. e.g. Contoso.Plugin.dll</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string Pattern { get; set; }
        #endregion

        #region Process Record
        protected override void ProcessRecord()
        {
            var query = new QueryExpression
            {
                EntityName = Workflow.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("category", ConditionOperator.Equal, (int)Workflow_Category.Workflow),
                        new ConditionExpression("type", ConditionOperator.In, new int[] {(int) Workflow_Type.Definition, (int) Workflow_Type.Template}),
                        new ConditionExpression("ismanaged", ConditionOperator.Equal, false),
                    }
                }
            };

            if (!string.IsNullOrEmpty(Name))
            {
                query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
            }
            else if (!string.IsNullOrEmpty(Pattern))
            {
                query.Criteria.AddCondition("name", ConditionOperator.Like, Pattern);
            }
            else 
            {
                throw new ArgumentException("You must provide Name or Pattern argument");
            }

            using (var context = new CIContext(OrganizationService))
            {
                var result = OrganizationService.RetrieveMultiple(query);

                if (result.Entities.Count == 0)
                {
                    WriteVerbose("Couldn't find matching workflows.");
                    return;
                }

                var pluginRegistrationHelper = new PluginRegistrationHelper(OrganizationService, context, WriteVerbose, WriteWarning);
                var deletingHashSet = new HashSet<string>();
                foreach (var wf in result.Entities.Select(x => x.ToEntity<Workflow>()))
                {
                    pluginRegistrationHelper.DeleteObjectWithDependencies(wf.Id, ComponentType.Workflow, deletingHashSet);
                    WriteVerbose($"Workflow {wf.Name} / {wf.Id} removed from CRM");
                }
            }
        }
        #endregion
    }
}