using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "XrmSolution")]
    public class GetXrmSolutionCommand : Cmdlet
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
        public string UniqueSolutionName
        {
            get { return uniqueSolutionName; }
            set { uniqueSolutionName = value; }
        }
        private string uniqueSolutionName;

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Exporting Solution: {0}", UniqueSolutionName));

            CrmConnection connection = CrmConnection.Parse(connectionString);

            using (OrganizationService service = new OrganizationService(connection))
            {

                using (CIContext context = new CIContext(service))
                {
                    var query = from s in context.SolutionSet
                                where s.UniqueName == UniqueSolutionName
                                select s;

                    Solution solution = query.FirstOrDefault<Solution>();

                    WriteObject(solution);
                }
            }
        }

        #endregion
    }
}
