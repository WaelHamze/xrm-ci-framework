using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Build.Client;
using System.Activities;
using Xrm.Framework.CI.Common;

namespace Xrm.Framework.CI.TeamFoundation.Activities
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class GetConnectionAttributes : CodeActivity
    {
        #region Arguments

        [RequiredArgument]
        public InArgument<string> CrmConnectionString { get; set; }

        public OutArgument<CrmConnectionAttributes> ConnectionAttributes { get; set; }

        #endregion

        #region Execute

        protected override void Execute(CodeActivityContext context)
        {
            ConnectionAttributes.Set(context, CrmConnectionConverter.ConvertFromConnectionString(CrmConnectionString.Get(context)));
        }

        #endregion
    }
}
