using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System.IO;
using Xrm.Framework.CI.Common.Entities;
using Microsoft.Xrm.Sdk.Messages;
using System.IO.Compression;
using Xrm.Framework.CI.Common;
using System.Xml.Linq;
using System.Threading;

namespace Xrm.Framework.CI.PowerShell
{
    [Cmdlet(VerbsData.Import, "XrmSolution")]
    public class ImportXrmSolutionCommand : Cmdlet
    {
        #region Parameters

        [Parameter(Mandatory = true)]
        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }
        private string connectionString;

        [Parameter(Mandatory = true)]
        public string SolutionFilePath
        {
            get { return solutionFilePath; }
            set { solutionFilePath = value; }
        }
        private string solutionFilePath;

        [Parameter(Mandatory = false)]
        public bool PublishWorkflows
        {
            get { return publishWorkflows; }
            set { publishWorkflows = value; }
        }
        private bool publishWorkflows = false;

        [Parameter(Mandatory = false)]
        public bool ConvertToManaged
        {
            get { return convertToManaged; }
            set { convertToManaged = value; }
        }
        private bool convertToManaged = false;

        [Parameter(Mandatory = false)]
        public bool OverwriteUnmanagedCustomizations
        {
            get { return overwriteUnmanagedCustomizations; }
            set { overwriteUnmanagedCustomizations = value; }
        }
        private bool overwriteUnmanagedCustomizations = false;

        [Parameter(Mandatory = false)]
        public bool SkipProductUpdateDependencies
        {
            get { return skipProductUpdateDependencies; }
            set { skipProductUpdateDependencies = value; }
        }
        private bool skipProductUpdateDependencies = false;

        [Parameter(Mandatory = false)]
        public bool ImportAsync
        {
            get { return importAsync; }
            set { importAsync = value; }
        }
        private bool importAsync = false;

        [Parameter(Mandatory = false)]
        public bool WaitForCompletion
        {
            get { return waitForCompletion; }
            set { waitForCompletion = value; }
        }
        private bool waitForCompletion = false;

        [Parameter(Mandatory = false)]
        public int SleepInterval
        {
            get { return sleepInterval; }
            set { sleepInterval = value; }
        }
        private int sleepInterval = 15;

        [Parameter(Mandatory = false)]
        public int AsyncWaitTimeout
        {
            get { return asyncWaitTimeout; }
            set { asyncWaitTimeout = value; }
        }
        private int asyncWaitTimeout = 15*60;

        [Parameter(Mandatory = false)]
        public Guid ImportJobId
        {
            get { return importJobId; }
            set { importJobId = value; }
        }
        private Guid importJobId = Guid.Empty;

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Importing Solution: {0}", SolutionFilePath));

            CrmConnection connection = CrmConnection.Parse(connectionString);

            if (importJobId == Guid.Empty)
            {
                ImportJobId = Guid.NewGuid();
            }

            base.WriteVerbose(string.Format("ImportJobId {0}", ImportJobId));

            using (OrganizationService service = new OrganizationService(connection))
            {
                byte[] solutionBytes = File.ReadAllBytes(solutionFilePath);
                
                ImportSolutionRequest importSolutionRequest = new ImportSolutionRequest()
                {
                    CustomizationFile = solutionBytes,
                    PublishWorkflows = publishWorkflows,
                    ConvertToManaged = convertToManaged,
                    OverwriteUnmanagedCustomizations = overwriteUnmanagedCustomizations,
                    SkipProductUpdateDependencies = skipProductUpdateDependencies,
                    ImportJobId = ImportJobId,
                    RequestId = ImportJobId
                };

                if (importAsync)
                {
                    Guid asyncJobId;
                    
                    ExecuteAsyncRequest asyncRequest = new ExecuteAsyncRequest()
                    {
                        Request = importSolutionRequest,
                        RequestId = ImportJobId
                    };
                    ExecuteAsyncResponse asyncResponse = service.Execute(asyncRequest) as ExecuteAsyncResponse;

                    asyncJobId = asyncResponse.AsyncJobId;

                    WriteObject(asyncJobId);

                    if (waitForCompletion)
                    {
                        DateTime end = DateTime.Now.AddSeconds(asyncWaitTimeout);

                        bool completed = false;
                        while (!completed)
                        {
                            if (end < DateTime.Now)
                            {
                                throw new Exception(string.Format("Import Timeout Exceeded: {0}", asyncWaitTimeout));
                            }

                            Thread.Sleep(SleepInterval*1000);

                            AsyncOperation asyncOperation;

                            try
                            {

                                asyncOperation = service.Retrieve("asyncoperation", asyncJobId,
                                    new ColumnSet("asyncoperationid", "statuscode", "message")).ToEntity<AsyncOperation>();
                            }
                            catch (Exception ex)
                            {
                                base.WriteWarning(ex.Message);
                                continue;
                            }

                            switch (asyncOperation.StatusCode.Value)
                            {
                                //Succeeded
                                case 30:
                                    completed = true;
                                    break;
                                //Pausing //Canceling //Failed //Canceled
                                case 21:
                                case 22:
                                case 31:
                                case 32:
                                    throw new Exception(string.Format("Solution Import Failed: {0} {1}",
                                        asyncOperation.StatusCode.Value, asyncOperation.Message));
                                default:
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    ImportSolutionResponse importSolutionResponse = service.Execute(importSolutionRequest) as ImportSolutionResponse;
                }
                
                base.WriteVerbose(string.Format("{0} Imported Completed {1}", SolutionFilePath, importJobId));
            }
        }

        #endregion
    }
}
