using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Xrm.Framework.CI.Common.Entities;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;

namespace Xrm.Framework.CI.TeamFoundation.Activities
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class GetSolutionDetails : CodeActivity
    {
        #region Arguments

        [RequiredArgument]
        public InArgument<string> CrmConnectionString { get; set; }

        [RequiredArgument]
        public InArgument<string> UniqueSolutionName { get; set; }

        public OutArgument<Solution> SolutionDetails { get; set; }

        #endregion
        
        #region Execute

        protected override void Execute(CodeActivityContext context)
        {
            CrmConnection connection = CrmConnection.Parse(CrmConnectionString.Get(context));


            using (OrganizationService service = new OrganizationService(connection))
            {
                using (CIContext ciContext = new CIContext(service))
                {
                    var query = from s in ciContext.SolutionSet
                                where s.UniqueName == UniqueSolutionName.Get(context)
                                select s;
                    Solution solution = query.FirstOrDefault<Solution>();

                    if (solution == null)
                    {
                        throw new Exception(string.Format("Solution {0} could not be found", UniqueSolutionName));
                    }

                    SolutionDetails.Set(context, solution);
                }
            }
        }

        #endregion
    }
}
