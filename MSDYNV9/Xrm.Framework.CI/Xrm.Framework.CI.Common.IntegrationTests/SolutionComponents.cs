using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Xrm.Framework.CI.Common.IntegrationTests.Logging;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common.IntegrationTests
{
    [TestClass]
    public class SolutionComponents
    {
        #region Properties

        public ILogger Logger
        {
            get;
            set;
        }

        public IOrganizationService OrganizationService
        {
            get;
            set;
        }

        public TestContext TestContext
        {
            get;
            set;
        }

        public string ArtifactsDirectory
        {
            get;
            set;
        }

        public string LogsDirectory
        {
            get;
            set;
        }

        public SolutionComponentsManager Manager
        {
            get;
            set;
        }

        #endregion

        [TestInitialize()]
        public void Setup()
        {
            Logger = new TestLogger();
            OrganizationService = new TestConnectionManager().CreateConnection();
            ArtifactsDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Artifacts";
            LogsDirectory = TestContext.TestLogsDir;
            Manager = new SolutionComponentsManager(Logger, OrganizationService);
        }

        [TestMethod]
        public void GetMissingComponents_Found()
        {
            MissingComponent[] components = Manager.GetMissingComponentsOnTarget($"{ArtifactsDirectory}\\MissingDependency_1_0_0_0_managed.zip");

            Assert.AreEqual(components.Length > 0, true);

            string jsonFile = $"{LogsDirectory}\\MissingComponents.json";

            Serializers.SaveJson<MissingComponent[]>(jsonFile, components);
        }

        [TestMethod]
        public void GetMissingComponents_NotFound()
        {
            MissingComponent[] components = Manager.GetMissingComponentsOnTarget($"{ArtifactsDirectory}\\Success_1_0_0_0_managed.zip");

            Assert.AreEqual(components.Length == 0, true);
        }

        [TestMethod]
        public void GetMissingDependencies()
        {
            EntityCollection components = Manager.GetMissingDependencies("MissingDependency");

            Assert.AreEqual(components.Entities.Count > 0, true);

            string jsonFile = $"{LogsDirectory}\\MissingDependencies.json";

            Serializers.SaveJson<EntityCollection>(jsonFile, components);
        }
    }
}
