using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;
using Microsoft.Xrm.Sdk;

namespace Xrm.Framework.CI.PowerShell
{
    [Cmdlet(VerbsCommon.Set, "XrmSolutionVersion")]
    public class SetXrmSolutionVersionCommand : Cmdlet
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
        public string SolutionName
        {
            get { return solutionName; }
            set { solutionName = value; }
        }
        private string solutionName;

        [Parameter(Mandatory = true)]
        public string Version
        {
            get { return version; }
            set { version = value; }
        }
        private string version;

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Updating Solution {0} Version: {1}", SolutionName, Version));

            CrmConnection connection = CrmConnection.Parse(connectionString);

            Solution solution = null;

            using (OrganizationService orgService = new OrganizationService(connection))
            {
                using (CIContext context = new CIContext(orgService))
                {
                    var query = from s in context.SolutionSet
                                where s.UniqueName == solutionName
                                select s;

                    solution = query.FirstOrDefault<Solution>();
                }

                if (solution == null)
                {
                    throw new Exception(string.Format("Solution {0} could not be found", SolutionName));
                }
                else
                {
                    Solution update = new Solution();
                    update.Id = solution.Id;
                    update.Version = version;
                    orgService.Update(update);

                    base.WriteVerbose(string.Format("Solution {0} Update to Version: {1}", SolutionName, Version));
                }
            }
        }

        #endregion
    }
}
