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
            Debug.WriteLine(format, args);
        }

        public void LogInformation(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }

        public void LogVerbose(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }

        public void LogWarning(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }

        #endregion
    }
}
