using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public interface IAsyncStatusUpdate
    {
        void OnProgressUpdate(AsyncOperation asyncOperation);
    }

    public class AsyncOperationManager : XrmBase
    {
        #region Constructors

        public AsyncOperationManager(ILogger logger, IOrganizationService organizationService)
            : base(logger, organizationService)
        {
        }

        #endregion

        #region Methods

        public AsyncOperation AwaitCompletion(
            Guid asyncJobId,
            int asyncWaitTimeout,
            int sleepInterval,
            IAsyncStatusUpdate statusUpdate)
        {
            DateTime end = DateTime.Now.AddSeconds(asyncWaitTimeout);

            AsyncOperation asyncOperation = null;

            bool completed = false;
            while (!completed)
            {
                if (end < DateTime.Now)
                {
                    throw new Exception(string.Format("Import Timeout Exceeded: {0}", asyncWaitTimeout));
                }

                Logger.LogVerbose(string.Format("Sleeping for {0} seconds", sleepInterval));
                Thread.Sleep(sleepInterval * 1000);

                try
                {
                    asyncOperation = OrganizationService.Retrieve("asyncoperation", asyncJobId,
                        new ColumnSet("asyncoperationid", "statuscode", "message")).ToEntity<AsyncOperation>();

                    AsyncOperation_StatusCode status = (AsyncOperation_StatusCode)asyncOperation.StatusCode.Value;

                    Logger.LogVerbose("Async Operation Status: {0}", status.ToString());

                    if (statusUpdate != null)
                    {
                        statusUpdate.OnProgressUpdate(asyncOperation);
                    }

                    switch (status)
                    {
                        case AsyncOperation_StatusCode.Succeeded:
                        case AsyncOperation_StatusCode.Failed:
                        case AsyncOperation_StatusCode.Canceled:
                            return asyncOperation;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogVerbose(ex.Message);
                    continue;
                }
            }

            return asyncOperation;
        }

        #endregion
    }
}
