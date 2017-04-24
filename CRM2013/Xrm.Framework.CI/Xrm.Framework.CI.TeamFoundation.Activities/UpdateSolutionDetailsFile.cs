using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Xrm.Framework.CI.Common;

namespace Xrm.Framework.CI.TeamFoundation.Activities
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class UpdateSolutionDetailsFile : CodeActivity
    {
        #region Arguments

        [RequiredArgument]
        public InArgument<string> SolutionFile { get; set; }

        [RequiredArgument]
        public InArgument<string> SolutionVersion { get; set; }

        #endregion

        #region Execute

        protected override void Execute(CodeActivityContext context)
        {
            SolutionDetails details = SolutionDetails.Create(SolutionFile.Get(context));

            details.SolutionVersion = SolutionVersion.Get(context);

            details.Save();
        }

        #endregion
    }
}
