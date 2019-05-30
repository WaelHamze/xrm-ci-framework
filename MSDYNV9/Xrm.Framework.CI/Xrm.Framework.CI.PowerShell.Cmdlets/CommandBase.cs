using System.Management.Automation;
using Xrm.Framework.CI.Common.Logging;
using Xrm.Framework.CI.PowerShell.Cmdlets.Logging;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public abstract class CommandBase : PSCmdlet
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