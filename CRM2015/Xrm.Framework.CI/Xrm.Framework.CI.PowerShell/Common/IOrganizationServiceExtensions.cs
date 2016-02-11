using System;
using System.Collections.Generic;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Xrm.Framework.CI.PowerShell.Common
{
    public static class IOrganizationServiceExtension
    {
        public static IEnumerable<Entity> RetrieveAll(this IOrganizationService organizationService, FetchExpression query)
        {
            var request = new FetchXmlToQueryExpressionRequest { FetchXml = query.Query };
            var repsonse = organizationService.Execute<FetchXmlToQueryExpressionResponse>(request);
            return RetrieveAll(organizationService, repsonse.Query);
        }

        public static IEnumerable<Entity> RetrieveAll(this IOrganizationService organizationService, QueryExpression query)
        {
            int pageNumber = 1;
            var entities = new List<Entity>();

            bool moreRecords;
            do
            {
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = 5000
                };

                EntityCollection entityCollection = organizationService.RetrieveMultiple(query);
                entities.AddRange(entityCollection.Entities);
                moreRecords = entityCollection.MoreRecords;
                pageNumber++;

            } while (moreRecords);

            return entities;
        }

        public static Entity Retrieve(this IOrganizationService organizationService, EntityReference entityReference, ColumnSet columnSet)
        {
            return organizationService.Retrieve(entityReference.LogicalName, entityReference.Id, columnSet);
        }

        public static void UpdateEntityField<TAttributeType>(this IOrganizationService organizationService, string entityName, Guid entityId, string attributeName, TAttributeType attributeValue)
        {
            var entity = new Entity(entityName) { Id = entityId };
            entity[attributeName] = attributeValue;
            organizationService.Update(entity);
        }
    }
}
