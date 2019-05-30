using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.Common.Workflows;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Set the state for the existing workflow.</para>
    /// <para type="description">The Set-XrmWorkflowState cmdlet attempts to publish/unpublish an existing workflow or workflows in CRM. This cmdlet will attempt to fix broken references in the workflow during publishing.</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Set-XrmWorkflowState -Name "My First Workflow" -Activated $false</code>
    ///   <code>C:\PS>Set-XrmWorkflowState -Pattern "ISV%" -Activated $true</code>
    ///   <para>Workflow Name Pattern to publish/unpublish (activate/deactivate)</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "XrmWorkflowState")]
    public class SetXrmWorkflowStateCommand : XrmCommandBase
    {
        #region Parameters
        private WorkflowFixer workflowfixer;
        private const uint ErrorsInWorkflowDefinition = 0x80048455;
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

        /// <summary>
        /// <para type="description">Specified workflows should be activated/deactivated</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public bool Activate { get; set; }
        #endregion

        #region Begin Processing
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            workflowfixer = new WorkflowFixer(OrganizationService, Logger);
        }
        #endregion

        #region Process Record
        protected override void ProcessRecord()
        {
            var sourceState = Activate ? WorkflowState.Draft : WorkflowState.Activated;
            var query = new QueryExpression
            {
                EntityName = Workflow.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("category", ConditionOperator.In, new[] {(int) Workflow_Category.Workflow, (int) Workflow_Category.BusinessProcessFlow, (int) Workflow_Category.Action}),
                        new ConditionExpression("type", ConditionOperator.In, new[] {(int) Workflow_Type.Definition, (int) Workflow_Type.Template}),
                        new ConditionExpression("statecode", ConditionOperator.Equal, (int) sourceState),
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

            var workflows = OrganizationService.RetrieveMultiple(query)
                .Entities
                .Select(x => x.ToEntity<Workflow>())
                .ToList();

            if (workflows.Count == 0)
            {
                WriteVerbose($"Couldn't find matching workflows in {sourceState} state.");
                return;
            }

            foreach (var workflow in workflows)
            {
                if (Activate)
                {
                    WriteVerbose($"Trying to publish {workflow.CategoryEnum} {workflow.Name}");
                    PublishWorkflow(workflow, true);
                }
                else
                {
                    WriteVerbose($"Trying to unpublish {workflow.CategoryEnum} {workflow.Name}");
                    UnpublishWorkflow(workflow);
                }
            }
        }

        private void PublishWorkflow(Workflow workflow, bool tryFixOnErrors)
        {
            try
            {
                PublishWorkflow(workflow);
                WriteVerbose(" ok");
            }
            catch (FaultException<OrganizationServiceFault> ex) when ((uint)ex.Detail.ErrorCode == ErrorsInWorkflowDefinition && tryFixOnErrors)
            {
                WriteWarning($" workflow {workflow.Name} has errors in definition");
                if (workflowfixer.FixWorkflow(workflow))
                {
                    PublishWorkflow(workflow, false);
                }
            }
            catch (Exception ex)
            {
                WriteWarning($" error during publishing workflow {workflow.Name}: {ex.Message}");
            }
        }

        private void PublishWorkflow(Workflow workflow) => OrganizationService.Execute(new SetStateRequest
        {
            EntityMoniker = workflow.ToEntityReference(),
            State = new OptionSetValue((int)WorkflowState.Activated),
            Status = new OptionSetValue((int)Workflow_StatusCode.Activated)
        });

        private void UnpublishWorkflow(Workflow workflow) => OrganizationService.Execute(new SetStateRequest
        {
            EntityMoniker = workflow.ToEntityReference(),
            State = new OptionSetValue((int)WorkflowState.Draft),
            Status = new OptionSetValue((int)Workflow_StatusCode.Draft)
        });
        #endregion
    }
}