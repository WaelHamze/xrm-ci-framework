using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.Common
{
    public class UpdateSolution
    {
        #region Properties

        public IOrganizationService OrgService { get; private set; }

        #endregion

        #region Constructors

        public UpdateSolution(IOrganizationService orgService)
        {
            OrgService = orgService;
        }

        #endregion

        #region Methods

        public void Update(string solutionName,
                            string version)
        {
            Solution solution = null;
            
            using (CIContext context = new CIContext(OrgService))
            {
                var query = from s in context.SolutionSet
                            where s.UniqueName == solutionName
                            select s;

                solution = query.FirstOrDefault<Solution>();
            }

            if (solution == null)
            {
                throw new Exception(string.Format("Solution {0} could not be found", solutionName));
            }
            else
            {
                Solution update = new Solution();
                update.Id = solution.Id;
                update.Version = version;
                OrgService.Update(update);
            }
        }

        #endregion
    }
}
