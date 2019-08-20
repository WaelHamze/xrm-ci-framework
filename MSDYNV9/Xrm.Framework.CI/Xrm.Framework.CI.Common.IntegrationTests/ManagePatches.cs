using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.Common.IntegrationTests.Logging;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common.IntegrationTests
{
    [TestClass]
    public class ManagePatches
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

        public IOrganizationService PollingOrganizationService
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

        public string LogFileName
        {
            get;
            set;
        }

        public SolutionManager SolutionManager
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
            PollingOrganizationService = new TestConnectionManager().CreateConnection();
            ArtifactsDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Artifacts";
            LogsDirectory = TestContext.TestLogsDir;
            LogFileName = $"{TestContext.TestName}.xml";
            SolutionManager = new SolutionManager(Logger, OrganizationService, PollingOrganizationService);
        }

        [TestMethod]
        public void TestPatches()
        {
            Solution solution = SolutionManager.CreateSolution(
                "UltraDynamics",
                "TestSolution",
                "Test Solution",
                "Solution Description",
                "1.0.0.0");

            Solution patch1 = SolutionManager.CreatePatch("TestSolution",
                                        string.Empty,
                                        string.Empty);

            Solution patch2 = SolutionManager.CreatePatch("TestSolution",
                            string.Empty,
                            string.Empty);

            List<Solution> patches = SolutionManager.GetSolutionPatches(
                "TestSolution");

            Assert.AreEqual(patches.Count, 2);

            Assert.AreEqual(patches[0].Version, "1.0.2.0");
            Assert.AreEqual(patches[1].Version, "1.0.1.0");

            SolutionManager.DeleteSolution(patch2.UniqueName);
            SolutionManager.DeleteSolution(patch1.UniqueName);
            SolutionManager.DeleteSolution("TestSolution");
        }
    }
}
