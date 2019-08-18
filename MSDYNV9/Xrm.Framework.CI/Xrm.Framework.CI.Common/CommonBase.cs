using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public class CommonBase
    {
        #region Properties

        protected ILogger Logger
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public CommonBase(ILogger logger)
        {
            Logger = logger;
        }

        #endregion
    }
}
