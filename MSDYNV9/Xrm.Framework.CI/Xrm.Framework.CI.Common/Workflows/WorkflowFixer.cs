using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common.Workflows
{
    public class WorkflowFixer {
        private const string GuidPropertyType = "Microsoft.Xrm.Sdk.Workflow.WorkflowPropertyType.Guid";
        private const string EntityReferencePropertyType = "Microsoft.Xrm.Sdk.Workflow.WorkflowPropertyType.EntityReference";
        private const string InArgumentXpath = "//a:InArgument";
        private const string OutArgumentXpath = "./a:OutArgument";
        private const string WindowsWorkflowFoundationNamespace = "http://schemas.microsoft.com/netfx/2009/xaml/activities";

        private readonly IOrganizationService service;
        private readonly ILogger logger;

        public WorkflowFixer(IOrganizationService service, ILogger logger)
        {
            this.service = service;
            this.logger = logger;
        }

        public bool FixWorkflow(Workflow workflow)
        {
            logger.LogVerbose($" Trying to fix workflow {workflow.Name}");
            var xaml = XDocument.Parse(workflow.Xaml);
            var nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("a", WindowsWorkflowFoundationNamespace);
            var entityReferences = xaml.XPathSelectElements(InArgumentXpath, nsmgr)
                .Where(x => x.Value.Contains(EntityReferencePropertyType) && x.Value.Contains("Lookup"))
                .Select(x => new
                {
                    EntityLogicalName = XamlValuesHelper.ExtractEntityLogicalNameFromLookup(x.Value),
                    Name = XamlValuesHelper.ExtractNameFromLookup(x.Value),
                    Parameter = x,
                    ReferenceName = XamlValuesHelper.ExtractGuidParameterNameFromLookup(x.Value)
                })
                .ToList();

            var guidParameters = xaml.XPathSelectElements(InArgumentXpath, nsmgr)
                .Where(x => x.Value.Contains(GuidPropertyType))
                .Select(x => new
                {
                    Name = x.Parent.XPathSelectElements(OutArgumentXpath, nsmgr).Single().Value.Trim('[', ']'),
                    Guid = XamlValuesHelper.ExtractGuidValueFromGuidParameter(x.Value),
                    Parameter = x,
                })
                .ToDictionary(x => x.Name);

            foreach (var entityReference in entityReferences)
            {
                var searchedId = guidParameters[entityReference.ReferenceName];
                var newId = FindRecord(entityReference.EntityLogicalName, entityReference.Name, searchedId.Guid);
                if (newId != null)
                {
                    if (searchedId.Guid != newId)
                    {
                        logger.LogVerbose($" Updating reference for parameter = '{entityReference.ReferenceName}' with type = '{entityReference.EntityLogicalName}', name = '{entityReference.Name}' from {searchedId.Guid} to {newId}");
                        searchedId.Parameter.Value = searchedId.Parameter.Value.Replace(searchedId.Guid.ToString(), newId.ToString());
                    }
                }
                else
                {
                    logger.LogVerbose($" Couldn't find record for parameter = '{entityReference.ReferenceName}' with type = '{entityReference.EntityLogicalName}', name = '{entityReference.Name}'");
                    return false;
                }
            }

            service.Update(new Workflow
            {
                Id = workflow.Id,
                Xaml = xaml.ToString()
            });
            return true;
        }

        private Guid? FindRecord(string entityLogicalName, string name, Guid id)
        {
            var metadata = service.GetEntityMetadata(entityLogicalName);
            var results = service.RetrieveMultiple(new QueryByAttribute
            {
                EntityName = entityLogicalName,
                Attributes = { metadata.PrimaryIdAttribute },
                Values = { id }
            });
            if (results.Entities.Count > 0)
            {
                return results.Entities.Single().Id;
            }
            results = service.RetrieveMultiple(new QueryByAttribute
            {
                EntityName = entityLogicalName,
                Attributes = { metadata.PrimaryNameAttribute },
                Values = { name }
            });
            if (results.Entities.Count == 0)
            {
                logger.LogWarning($" Can't find matching records for {entityLogicalName}.{metadata.PrimaryNameAttribute} = '{name}'");
            }
            if (results.Entities.Count > 1)
            {
                logger.LogWarning($" There are more than one matching records for {entityLogicalName}.{metadata.PrimaryNameAttribute} = '{name}'. Picking first.");
            }
            return results.Entities.FirstOrDefault()?.Id;
        }
    }
}