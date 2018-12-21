using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.PowerShell.Cmdlets.Logging
{
    public class PSLogger : ILogger
    {
        #region Properties

        protected XrmCommandBase XrmCmdlet
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public PSLogger(XrmCommandBase xrmCmdlet)
        {
            XrmCmdlet = xrmCmdlet;
        }

        #endregion

        #region ILogger

        public void LogError(string format, params object[] args)
        {
            XrmCmdlet.WriteVerbose(string.Format(format, args));
        }

        public void LogInformation(string format, params object[] args)
        {
            XrmCmdlet.WriteVerbose(string.Format(format, args));
        }

        public void LogVerbose(string format, params object[] args)
        {
            XrmCmdlet.WriteVerbose(string.Format(format, args));
        }

        public void LogWarning(string format, params object[] args)
        {
            XrmCmdlet.WriteVerbose(string.Format(format, args));
        }

        #endregion
    }
}
