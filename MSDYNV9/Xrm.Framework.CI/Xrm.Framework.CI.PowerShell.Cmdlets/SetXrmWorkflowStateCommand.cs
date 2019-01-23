using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using Microsoft.Xrm.Sdk.Metadata;

using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Reset a workflow.</para>
    /// <para type="description">The Reset-XrmWorkflow cmdlet try to publish an existing workflow or workflows in CRM.
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Reset-XrmWorkflow -Name $workflowNamePattern</code>
    ///   <para>Workflow Name Pattern to Reset</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "XrmWorkflowState")]
    public class SetXrmWorkflowStateCommand : XrmCommandBase
    {
        #region Parameters

        private const string findByName = "FindByName";
        private const string findByPattern = "FindByPattern";

        /// <summary>
        /// <para type="description">The assembly name. e.g. Contoso.Plugin.dll</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = findByName)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The assembly name. e.g. Contoso.Plugin.dll</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = findByPattern)]
        public string Pattern { get; set; }

        /// <summary>
        /// <para type="description">The assembly name. e.g. Contoso.Plugin.dll</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public bool Activated { get; set; }
        #endregion

        #region Process Record
        protected override void ProcessRecord()
        {
            var sourceState = Activated ? WorkflowState.Draft : WorkflowState.Activated;
            var query = new QueryExpression
            {
                EntityName = Workflow.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("category", ConditionOperator.In, new [] {(int)Workflow_Category.Workflow, (int) Workflow_Category.BusinessProcessFlow}),
                        new ConditionExpression("type", ConditionOperator.In, new [] {(int) Workflow_Type.Definition, (int) Workflow_Type.Template}),
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
                if (Activated)
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
        #endregion

        const uint ErrorsInWorkflowDefinition = 0x80048455;
        const string GuidPropertyType = "Microsoft.Xrm.Sdk.Workflow.WorkflowPropertyType.Guid";
        const string EntityReferencePropertyType = "Microsoft.Xrm.Sdk.Workflow.WorkflowPropertyType.EntityReference";
        const string WindowsWorkflowFoundationNamespace = "http://schemas.microsoft.com/netfx/2009/xaml/activities";

        private string ExtractField(string s, int n)
        {
            var tmp = s.Substring(s.IndexOf("{") + 1, s.LastIndexOf("}") - s.IndexOf("{") - 1).Trim();
            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tmp)))
            {
                using (var parser = new TextFieldParser(stream))
                {
                    parser.SetDelimiters(",");
                    var arr = parser.ReadFields();
                    return arr.Length >= n ? arr[n] : null;
                }
            }
        }

        private Guid ExtractGuid(string s) => Guid.Parse(ExtractField(s, 1));

        private string ExtractName(string s) => ExtractField(s, 2);

        private string ExtractEntityLogicalName(string s) => ExtractField(s, 1);

        private string ExtractGuidParameterName(string s) => ExtractField(s, 3);

        private Guid? RecordLookup(string entityLogicalName, string name, Guid id)
        {
            var metadata = OrganizationService.GetEntityMetadata(entityLogicalName);
            var results = OrganizationService.RetrieveMultiple(new QueryByAttribute
            {
                EntityName = entityLogicalName,
                Attributes = { metadata.PrimaryIdAttribute },
                Values = { id }
            });
            if (results.Entities.Count > 0)
            {
                return results.Entities.First().Id;
            }
            results = OrganizationService.RetrieveMultiple(new QueryByAttribute
            {
                EntityName = entityLogicalName,
                Attributes = { metadata.PrimaryNameAttribute },
                Values = { name }
            });
            return results.Entities.FirstOrDefault()?.Id;
        }

        private bool FixWorkflow(Workflow workflow)
        {
            WriteVerbose($" Trying to fix workflow {workflow.Name}");
            var xaml = XDocument.Parse(workflow.Xaml);
            var nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("a", WindowsWorkflowFoundationNamespace);
            var entityReferences = xaml.XPathSelectElements("//a:InArgument", nsmgr)
                .Where(x => x.Value.Contains(EntityReferencePropertyType) && x.Value.Contains("Lookup"))
                .Select(x => new
                {
                    EntityLogicalName = ExtractEntityLogicalName(x.Value),
                    Name = ExtractName(x.Value),
                    Parameter = x,
                    ReferenceName = ExtractGuidParameterName(x.Value)
                })
                .ToList();

            var guidParameters = xaml.XPathSelectElements("//a:InArgument", nsmgr)
                .Where(x => x.Value.Contains(GuidPropertyType))
                .Select(x => new
                {
                    Name = x.Parent.XPathSelectElements("./a:OutArgument", nsmgr).SingleOrDefault().Value.Trim('[', ']'),
                    Guid = ExtractGuid(x.Value),
                    Parameter = x,
                })
                .ToDictionary(x => x.Name);

            foreach (var entityReference in entityReferences)
            {
                var searchedId = guidParameters[entityReference.ReferenceName];
                var newId = RecordLookup(entityReference.EntityLogicalName, entityReference.Name, searchedId.Guid);
                if (newId != null)
                {
                    if (searchedId.Guid != newId)
                    {
                        WriteVerbose($" Updating reference for parameter = '{entityReference.ReferenceName}' with type = '{entityReference.EntityLogicalName}', name = '{entityReference.Name}' from {searchedId.Guid} to {newId}");
                        searchedId.Parameter.Value = searchedId.Parameter.Value.Replace(searchedId.Guid.ToString(), newId.ToString());
                    }
                }
                else
                {
                    WriteWarning($" Couldn't find record for parameter = '{entityReference.ReferenceName}' with type = '{entityReference.EntityLogicalName}', name = '{entityReference.Name}'");
                    return false;
                }
            }

            OrganizationService.Update(new Workflow
            {
                Id = workflow.Id,
                Xaml = xaml.ToString()
            });
            return true;
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
                if (FixWorkflow(workflow))
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
    }
}