using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Reset a workflow.</para>
    /// <para type="description">The Reset-XrmWorkflow cmdlet try an existing workflow or workflows in CRM.
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Reset-XrmWorkflow -Name $workflowNamePattern</code>
    ///   <para>Workflow Name Pattern to Remove</para>
    /// </example>
    [Cmdlet(VerbsCommon.Reset, "XrmWorkflow")]
    public class ResetXrmWorkflow : XrmCommandBase
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
                        new ConditionExpression(Workflow.Fields.Category, ConditionOperator.Equal, (int)Workflow_Category.Workflow),
                        new ConditionExpression(Workflow.Fields.Type, ConditionOperator.In, new int[] {(int) Workflow_Type.Definition, (int) Workflow_Type.Template}),
                        new ConditionExpression(Workflow.Fields.IsManaged, ConditionOperator.Equal, false),
                        new ConditionExpression(Workflow.Fields.StateCode, ConditionOperator.Equal, (int) WorkflowState.Draft),
                    }
                }
            };

            if (!string.IsNullOrEmpty(Name))
            {
                query.Criteria.AddCondition(Workflow.Fields.Name, ConditionOperator.Equal, Name);
            }
            else if (!string.IsNullOrEmpty(Pattern))
            {
                query.Criteria.AddCondition(Workflow.Fields.Name, ConditionOperator.Like, Pattern);
            }
            else 
            {
                throw new ArgumentException("You must provide Name or Pattern argument");
            }

            var result = OrganizationService.RetrieveMultiple(query);

            if (result.Entities.Count == 0)
            {
                WriteVerbose("Couldn't find matching workflows in draft state.");
                return;
            }

            foreach (var wf in result.Entities.Select(x => x.ToEntity<Workflow>()))
            {
                PublishWorkflow(wf, true);
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

        //  Microsoft.Xrm.Sdk.Workflow.WorkflowPropertyType.EntityReference, "team", "DSR", AssignStep3_2, "Lookup" }
        private string ExtractEntityLogicalName(string s) => ExtractField(s, 1);

        //  Microsoft.Xrm.Sdk.Workflow.WorkflowPropertyType.EntityReference, "team", "DSR", AssignStep3_2, "Lookup" }
        private string ExtractGuidParameterName(string s) => ExtractField(s, 3);

        private Guid? RecordLookup(string entityLogicalName, string name, Guid id)
        {
            var metadata = ((RetrieveEntityResponse) OrganizationService.Execute(new RetrieveEntityRequest
            {
                LogicalName = entityLogicalName,
                EntityFilters = Microsoft.Xrm.Sdk.Metadata.EntityFilters.Entity
            })).EntityMetadata;
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
            WriteVerbose($"Trying to fix workflow {workflow.Name}");
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
            WriteVerbose($"Trying to publish {workflow.CategoryEnum} {workflow.Name}");
            try
            {
                PublishWorkflow(workflow);
                WriteVerbose(" - ok");
            }
            catch (FaultException<OrganizationServiceFault> ex) when ((uint)ex.Detail.ErrorCode == ErrorsInWorkflowDefinition && tryFixOnErrors)
            {
                WriteWarning($" - workflow has errors in definition");
                if (FixWorkflow(workflow))
                {
                    PublishWorkflow(workflow, false);
                }
            }
            catch (Exception ex)
            {
                WriteWarning($" - error: {ex.Message}");
            }
        }

        private void PublishWorkflow(Workflow workflow) => OrganizationService.Execute(new SetStateRequest
        {
            EntityMoniker = workflow.ToEntityReference(),
            State = new OptionSetValue((int)WorkflowState.Activated),
            Status = new OptionSetValue((int)Workflow_StatusCode.Activated)
        });
    }
}