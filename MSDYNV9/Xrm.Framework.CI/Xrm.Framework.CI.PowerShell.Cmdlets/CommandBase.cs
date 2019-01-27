using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Management.Automation;
using System.Net;
using System.Threading;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Logging;
using Xrm.Framework.CI.PowerShell.Cmdlets.Logging;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public abstract class CommandBase : Cmdlet
    {
        protected virtual ILogger Logger { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            Logger = new PSLogger(this);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}