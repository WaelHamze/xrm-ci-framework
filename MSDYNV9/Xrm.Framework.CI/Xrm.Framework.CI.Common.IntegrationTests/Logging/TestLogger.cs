using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common.IntegrationTests.Logging
{
    public class TestLogger : ILogger
    {
        #region Constructors

        public TestLogger()
        {
        }

        #endregion

        #region ILogger

        public void LogError(string format, params object[] args)
        {
            string msg = PrefixMsg("Error: ", format);
            if (args.Length != 0)
            {
                Debug.WriteLine(msg, args);
            }
            else
            {
                Debug.WriteLine(msg);
            }
        }

        public void LogInformation(string format, params object[] args)
        {
            if (args.Length != 0)
            {
                Debug.WriteLine(format, args);
            }
            else
            {
                Debug.WriteLine(format);
            }
        }

        public void LogVerbose(string format, params object[] args)
        {
            string msg = PrefixMsg("Verbose: ", format);
            if (args.Length != 0)
            {
                Debug.WriteLine(msg, args);
            }
            else
            {
                Debug.WriteLine(msg);
            }
        }

        public void LogWarning(string format, params object[] args)
        {
            string msg = PrefixMsg("Warning: ", format);
            if (args.Length != 0)
            {
                Debug.WriteLine(msg, args);
            }
            else
            {
                Debug.WriteLine(msg);
            }
        }

        private string PrefixMsg(string prefix, string msg)
        {
            return $"{prefix}{msg}";
        }

        #endregion
    }
}
