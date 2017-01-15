using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xrm.Framework.CI.Sample.WFActivities
{
    public class SampleActivity : CodeActivity
    {

        #region Input Parameters

        [Input("Account")]
        [ReferenceTarget("account")]
        public InArgument<EntityReference> AccountReference { get; set; }

        #endregion

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            throw new InvalidPluginExecutionException("Sample Account WF Activity");
        }
    }
}
