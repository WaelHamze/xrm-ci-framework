using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public class XrmBase
    {
        #region Properties

        protected ILogger Logger
        {
            get;
            set;
        }

        protected IOrganizationService OrganizationService
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public XrmBase(ILogger logger, IOrganizationService organizationService)
        {
            Logger = logger;
            OrganizationService = organizationService;
        }

        #endregion
    }
}
