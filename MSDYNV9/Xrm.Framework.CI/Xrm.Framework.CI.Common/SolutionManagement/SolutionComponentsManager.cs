using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public class SolutionComponentsManager : XrmBase
    {
        #region Variables

        private const string ImportSuccess = "success";
        private const string ImportFailure = "failure";
        private const string ImportProcessed = "processed";
        private const string ImportUnprocessed = "Unprocessed";

        #endregion

        #region Properties

        protected IOrganizationService PollingOrganizationService
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public SolutionComponentsManager(ILogger logger,
            IOrganizationService organizationService)
            : base(logger, organizationService)
        {

        }

        #endregion

        #region Methods

        public MissingComponent[] GetMissingComponentsOnTarget(
                string solutionFilePath)
        {
            Logger.LogInformation("Retrieving Missing Components for  Solution: {0}", solutionFilePath);

            if (!File.Exists(solutionFilePath))
            {
                Logger.LogError("Solution File does not exist: {0}", solutionFilePath);
                throw new FileNotFoundException("Solution File does not exist", solutionFilePath);
            }

            SolutionXml solutionXml = new SolutionXml(Logger);

            XrmSolutionInfo info = solutionXml.GetSolutionInfoFromZip(solutionFilePath);

            if (info == null)
            {
                throw new Exception("Invalid Solution File");
            }
            else
            {
                Logger.LogInformation("Solution Unique Name: {0}, Version: {1}",
                    info.UniqueName,
                    info.Version);
            }

            byte[] solutionBytes = File.ReadAllBytes(solutionFilePath);

            var request = new RetrieveMissingComponentsRequest()
            {
                CustomizationFile = solutionBytes
            };

            RetrieveMissingComponentsResponse response = OrganizationService.Execute(request) as RetrieveMissingComponentsResponse;

            Logger.LogInformation("{0} Missing Components retrieved for Solution", response.MissingComponents.Length);

            return response.MissingComponents;
        }

        public EntityCollection GetMissingDependencies(
            string SolutionName)
        {
            Logger.LogInformation("Retrieving Missing Dependencies for Solution: {0}", SolutionName);

            if (string.IsNullOrEmpty(SolutionName))
            {
                throw new Exception("SolutionName is required to retrieve missing dependencies");
            }

            var request = new RetrieveMissingDependenciesRequest()
            {
                SolutionUniqueName = SolutionName
            };

            RetrieveMissingDependenciesResponse response = OrganizationService.Execute(request) as RetrieveMissingDependenciesResponse;

            Logger.LogInformation("{0} Missing dependencies retrieved for Solution", response.EntityCollection.Entities.Count);

            return response.EntityCollection;
        }

        #endregion
    }

    //public Component
}
