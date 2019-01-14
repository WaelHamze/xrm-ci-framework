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
            XrmCmdlet.WriteVerbose(string.Format("PS Version: {0}", GetPSVersion().ToString()));
            if (GetPSVersion().Major < 5)
            {
                XrmCmdlet.WriteVerbose("Switching to Console.WriteLine instead of WriteInformation due to PS Version");
            }
        }

        #endregion

        #region ILogger

        public void LogError(string format, params object[] args)
        {
            string message = string.Format(format, args);
            ErrorRecord error = new ErrorRecord(
                new Exception(message),
                "XrmCIFramework", ErrorCategory.WriteError, null);
            XrmCmdlet.WriteError(error);
        }

        public void LogInformation(string format, params object[] args)
        {
            string message = string.Format(format, args);
            if (GetPSVersion().Major < 5)
            {
                Console.WriteLine(message);
            }
            else
            {
                XrmCmdlet.WriteInformation(message, new string[] { "XrmCIFramework" });
            }
        }

        public void LogVerbose(string format, params object[] args)
        {
            string message = string.Format(format, args);
            XrmCmdlet.WriteVerbose(message);
        }

        public void LogWarning(string format, params object[] args)
        {
            string message = string.Format(format, args);
            XrmCmdlet.WriteWarning(message);
        }

        #endregion

        #region Private Methods

        private Version GetPSVersion()
        {
            return XrmCmdlet.CommandRuntime.Host.Version;
        }

        #endregion
    }
}
