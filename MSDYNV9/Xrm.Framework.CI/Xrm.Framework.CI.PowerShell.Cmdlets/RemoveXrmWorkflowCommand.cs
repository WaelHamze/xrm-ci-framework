using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Removes the workflow or workflows in CRM.</para>
    /// <para type="description">The Remove-XrmWorkflow cmdlet removes an existing workflow or workflows in CRM with dependencies.</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Remove-XrmWorkflow -Name "My First Workflow"</code>
    ///   <code>C:\PS>Remove-XrmWorkflow -Pattern "ISV%"</code>
    ///   <para>Workflow Name or Pattern to Remove</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "XrmWorkflow")]
    public class RemoveXrmWorkflowCommand : XrmCommandBase
    {
        #region Parameters

        private const string findByName = "FindByName";
        private const string findByPattern = "FindByPattern";

        /// <summary>
        /// <para type="description">Name of Workflow to delete, eq. "My First Workflow"</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = findByName)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">Pattern of Workflows to delete, eq. "ISV%</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = findByPattern)]
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
            else 
            {
                query.Criteria.AddCondition("name", ConditionOperator.Like, Pattern);
            }

            using (var context = new CIContext(OrganizationService))
            {
                var workflows = OrganizationService.RetrieveMultiple(query)
                    .Entities
                    .Select(x => x.ToEntity<Workflow>())
                    .ToList();

                if (workflows.Count == 0)
                {
                    WriteVerbose("Couldn't find matching unmanaged workflows.");
                    return;
                }

                var pluginRegistrationHelper = new PluginRegistrationHelper(OrganizationService, context, WriteVerbose, WriteWarning);
                var deletingHashSet = new HashSet<string>();
                foreach (var wf in workflows)
                {
                    pluginRegistrationHelper.DeleteObjectWithDependencies(wf.Id, ComponentType.Workflow, deletingHashSet);
                    WriteVerbose($"Workflow {wf.Name} / {wf.Id} removed from CRM");
                }
            }
        }
        #endregion
    }
}