using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xrm.Framework.CI.Common.Logging
{
    public interface ILogger
    {
        void LogVerbose(string format, params object[] args);
        void LogInformation(string format, params object[] args);
        void LogWarning(string format, params object[] args);
        void LogError(string format, params object[] args);
    }
}
