using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves a collection of records.</para>
    /// <para type="description">The Get-XrmEntity cmdlet retrieves a collection of records that satisfy the specified query criteria or all records from an entity.</para>
    /// <para type="description"> Executes one ore more "RetrieveMultipleRequest". The Cmdlet considers that all matching records are returned by handling the paging automatically.</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Get-XrmEntities -EntityName "account" -Attribute "firstname" -ConditionOperator Equals -Value "Sample"</code>
    ///   <para>Retrieves a collection of account records which matches a criteria. </para>
    /// </example>
    /// <example>
    ///   <code>C:\PS>Get-XrmEntities -EntityName "account"</code>
    ///   <para>Retrieves all account records. </para>
    /// </example>
    /// <example>
    ///   <code>C:\PS>Get-XrmEntities -FetchXml $fetchXml</code>
    ///   <para>Retrieves a collection of records by FetchXml.</para>
    /// </example>
    /// <para type="link" uri="http://msdn.microsoft.com/en-us/library/microsoft.xrm.sdk.messages.retrievemultiplerequest">RetrieveMultipleRequest.</para>
    [Cmdlet(VerbsCommon.Get, "XrmEntities")]
    [OutputType(typeof(IEnumerable<Entity>))]
    public class GetXrmEntitiesCommand : XrmCommandBase
    {
        /// <summary>
        /// <para type="description">The logical name of the entity to get the records from.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "QueryExpression")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "AllExpression")]
        public string EntityName { get; set; }

        /// <summary>
        /// <para type="description">The logical name of the attribute to look for when the query is executed.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "QueryExpression")]
        public string Attribute { get; set; }

        /// <summary>
        /// <para type="description">The type of comparison for two values in the query (Attribute and value or only attribute).</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = "QueryExpression")]
        public ConditionOperator ConditionOperator { get; set; }

        /// <summary>
        /// <para type="description">The attribute value to look for when the query is executed.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 3, ParameterSetName = "QueryExpression")]
        public object Value { get; set; }

        /// <summary>
        /// <para type="description">The FetchXml query that determines the set of records to retrieve.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FetchExpression")]
        public string FetchXml { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            IEnumerable<Entity> result;

            // Its ParameterSet FetchExpression
            if (!String.IsNullOrEmpty(FetchXml))
            {
                base.WriteVerbose("Retrieving Entities using FetchXml");

                var fetchXmlToQueryExpressionRequest = new FetchXmlToQueryExpressionRequest { FetchXml = FetchXml };
                var response = (FetchXmlToQueryExpressionResponse)OrganizationService.Execute(fetchXmlToQueryExpressionRequest);
                var query = response.Query;
                result = base.OrganizationService.RetrieveMultiple(query).Entities;
            }
            // Its ParameterSet QueryExpression
            else if (!String.IsNullOrEmpty(Attribute))
            {
                base.WriteVerbose("Retrieving Entities using QueryExpression by Attribute");

                var query = new QueryExpression(EntityName) { ColumnSet = new ColumnSet(true) };

                // Its possible to choose a ConditionOperator which does not need a value.
                // e.g. for "NotNull" you must not add a condition with a value also not if the value is null.
                if (Value == null)
                {
                    query.Criteria.AddCondition(Attribute, ConditionOperator);
                }
                else
                {
                    query.Criteria.AddCondition(Attribute, ConditionOperator, Value);
                }
                result = base.OrganizationService.RetrieveMultiple(query).Entities;
            }
            // Its AllExpression
            else
            {
                base.WriteVerbose("Retrieving all Entities using QueryExpression");

                result = OrganizationService.RetrieveMultiple(new QueryExpression(EntityName) { ColumnSet = new ColumnSet(true) }).Entities;
            }

            base.WriteVerbose(String.Format("{0} entities retrieved", result.Count()));

            WriteObject(result);
        }
    }
}
