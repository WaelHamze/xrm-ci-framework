using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "XrmBulkDeleteJob")]
    [OutputType(typeof(Guid))]
    public class NewXrmBulkDeleteJobCommand : XrmCommandBase
    {
        #region Parameters

        [Parameter(Mandatory = true)]
        public bool Async { get; set; }

        [Parameter(Mandatory = true)]
        public string[] QuerySet { get; set; }

        [Parameter(Mandatory = false)]
        public string BulkDeleteOperationName { get; set; }

        [Parameter(Mandatory = false)]
        public DateTime StartTime { get; set; }

        [Parameter(Mandatory = false)]
        public bool SendEmailNotification { get; set; }

        [Parameter(Mandatory = false)]
        public string[] ToRecipients { get; set; }

        [Parameter(Mandatory = false)]
        public string[] CcRecipients { get; set; }

        [Parameter(Mandatory = false)]
        public string RecurrencePattern { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            IEnumerable<QueryExpression> queries = GetQueryExpressionsFromFetchXml(QuerySet);
            var bulkDeleteRequest = CreateBulkDeleteRequest(queries);

            var response = (BulkDeleteResponse)OrganizationService.Execute(bulkDeleteRequest);
            WriteObject(response.JobId);
            if (!Async)
            {
                // NOTE: When the bulk delete operation was submitted, the GUID that was
                // returned was the asyncoperationid, not the bulkdeleteoperationid.
                AwaitJob(response.JobId);
            }
        }

        private IEnumerable<QueryExpression> GetQueryExpressionsFromFetchXml(IEnumerable<string> fetchXmlQueries)
        {
            foreach (var fetchXml in fetchXmlQueries)
            {
                var fetchXmlToQueryExpressionRequest = new FetchXmlToQueryExpressionRequest { FetchXml = fetchXml };
                var response = OrganizationService.Execute(fetchXmlToQueryExpressionRequest) as FetchXmlToQueryExpressionResponse;
                yield return response.Query;
            }
        }

        /// <summary>
        ///     Creates a <c>BulkDeleteRequest</c>.
        /// </summary>
        /// <param name="queries">The queries which returns the records to delete.</param>
        /// <returns>A <c>BulkDeleteRequest</c> object.</returns>
        private BulkDeleteRequest CreateBulkDeleteRequest(IEnumerable<QueryExpression> queries)
        {
            var bulkDeleteRequest = new BulkDeleteRequest
            {
                JobName = String.IsNullOrEmpty(BulkDeleteOperationName) ? "Bulk Delete - " + DateTime.Now.ToString("g") : BulkDeleteOperationName,
                QuerySet = queries.ToArray(),
                StartDateTime = StartTime == DateTime.MinValue ? DateTime.Now : StartTime, // Default is start immediately
                RecurrencePattern = RecurrencePattern ?? String.Empty, // Default is none (empty)
                SendEmailNotification = SendEmailNotification, // Default is false
                ToRecipients = ToRecipients == null ? new Guid[0] : ToRecipients.Select(Guid.Parse).ToArray(), // Default is none (empty)
                CCRecipients = CcRecipients == null ? new Guid[0] : CcRecipients.Select(Guid.Parse).ToArray(), // Default is none (empty)
            };
            return bulkDeleteRequest;
        }


        /// <summary>
        ///     Waits until a BulkDeleteOperation has completed.
        ///     This method will query (polling) for the BulkDeleteOperation until it has been completed.
        /// </summary>
        /// <param name="asyncOperationId">The asynchronous operation identifier.</param>
        /// <exception cref="System.Exception">Throws an exception if there are any failures in the BulkDeleteOperation</exception>
        private void AwaitJob(Guid asyncOperationId)
        {
            const int pollingTime = 10000;

            while (true)
            {
                // Grab the one bulk operation that has been created.
                BulkDeleteOperation createdBulkDeleteOperation = GetBulkDeleteOperation(asyncOperationId);

                // Check the operation's state.
                if (createdBulkDeleteOperation.StateCode.Value == BulkDeleteOperationState.Completed)
                {
                    // Stop polling as the operation's state is now complete.
                    if (createdBulkDeleteOperation.FailureCount != 0)
                    {
                        throw new Exception(String.Format("{0} Failures!", createdBulkDeleteOperation.FailureCount));
                    }
                    break;
                }

                Thread.Sleep(pollingTime);
            }
        }

        /// <summary>
        ///     Gets the bulk delete operation by the id of the async operation.
        /// </summary>
        /// <param name="asyncOperationId">The asynchronous operation identifier.</param>
        /// <returns>The BulkDeleteOperation.</returns>
        private BulkDeleteOperation GetBulkDeleteOperation(Guid asyncOperationId)
        {
            // Query for bulk delete operation and check for status.
            var bulkQuery = new QueryByAttribute(BulkDeleteOperation.EntityLogicalName) { ColumnSet = new ColumnSet(true) };
            bulkQuery.Attributes.Add("asyncoperationid");
            bulkQuery.Values.Add(asyncOperationId);

            EntityCollection entityCollection;
            do
            {
                // Wait a second for async operation to activate.
                Thread.Sleep(1000);
                entityCollection = OrganizationService.RetrieveMultiple(bulkQuery);
            } while (entityCollection.Entities.Count < 1);

            return entityCollection.Entities.Single().ToEntity<BulkDeleteOperation>();
        }

        #endregion
    }
}