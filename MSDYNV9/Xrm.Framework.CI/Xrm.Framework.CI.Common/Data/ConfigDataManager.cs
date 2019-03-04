using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public class ConfigDataManager : XrmBase
    {
        #region Variables


        #endregion

        #region Properties

        protected IOrganizationService PollingOrganizationService
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public ConfigDataManager(ILogger logger,
            IOrganizationService organizationService)
            : base(logger, organizationService)
        {

        }

        #endregion

        #region Methods

        public void UpdateData(
            string entityName,
            string lookupAttribute,
            List<string> attributesToUpdate,
            string dataJson)
        {
            string[][] data = JsonConvert.DeserializeObject<string[][]>(dataJson);
        }

        #endregion
    }

    //public Component
}
