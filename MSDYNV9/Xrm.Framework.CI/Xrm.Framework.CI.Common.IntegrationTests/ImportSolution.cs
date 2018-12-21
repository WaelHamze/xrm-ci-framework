using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Xrm.Framework.CI.Common.IntegrationTests.Logging;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common.IntegrationTests
{
    [TestClass]
    public class ImportSolution
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

        #endregion

        [TestInitialize()]
        public void Setup()
        {
            Logger = new TestLogger();
            OrganizationService = new XrmConnectionManager().CreateConnection();
        }

        [TestMethod]
        public void ImportSolutionAsyncSuccess()
        {
            SolutionManager solutionManager = 
                new SolutionManager(Logger, OrganizationService);

            SolutionImportResult result = solutionManager.ImportSolution(
                @"C:\Temp\TestPatches\BaseSolution_1_0_0_0_managed.zip",
                true,
                false,
                true,
                false,
                false,
                true,
                1,
                300,
                Guid.NewGuid(),
                true,
                @"C:\Temp\TestPatches",
                "Hi.xml");

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void ImportSolutionAsyncFail()
        {
            SolutionManager solutionManager =
                new SolutionManager(Logger, OrganizationService);

            SolutionImportResult result = solutionManager.ImportSolution(
                @"C:\Temp\TestPatches\Broken_1_0_0_0_managed.zip",
                true,
                false,
                true,
                false,
                false,
                true,
                1,
                300,
                Guid.NewGuid(),
                true,
                @"C:\Temp\TestPatches",
                "Hi.xml");

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void ImportSolutionSyncSuccess()
        {
            SolutionManager solutionManager =
                new SolutionManager(Logger, OrganizationService);

            SolutionImportResult result = solutionManager.ImportSolution(
                @"C:\Temp\TestPatches\BaseSolution_1_0_0_0_managed.zip",
                true,
                false,
                true,
                false,
                false,
                false,
                1,
                300,
                Guid.NewGuid(),
                true,
                @"C:\Temp\TestPatches",
                "Hi.xml");

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void ImportSolutionSyncFail()
        {
            SolutionManager solutionManager =
                new SolutionManager(Logger, OrganizationService);

            SolutionImportResult result = solutionManager.ImportSolution(
                @"C:\Temp\TestPatches\Broken_1_0_0_0_managed.zip",
                true,
                false,
                true,
                false,
                false,
                false,
                1,
                300,
                Guid.NewGuid(),
                true,
                @"C:\Temp\TestPatches",
                "Hi.xml");

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void ImportSolution_Sync_Fail_InvalidFile()
        {
            SolutionManager solutionManager =
                new SolutionManager(Logger, OrganizationService);

            SolutionImportResult result = solutionManager.ImportSolution(
                @"C:\Temp\TestPatches\PowerApps Governance and Deployment Whitepaper.pdf",
                true,
                false,
                true,
                false,
                false,
                false,
                1,
                300,
                Guid.NewGuid(),
                true,
                @"C:\Temp\TestPatches",
                "Hi.xml");

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void ImportSolution_Async_Fail_InvalidFile()
        {
            SolutionManager solutionManager =
                new SolutionManager(Logger, OrganizationService);

            SolutionImportResult result = solutionManager.ImportSolution(
                @"C:\Temp\TestPatches\PowerApps Governance and Deployment Whitepaper.pdf",
                true,
                false,
                true,
                false,
                false,
                true,
                1,
                300,
                Guid.NewGuid(),
                true,
                @"C:\Temp\TestPatches",
                "Hi.xml");

            Assert.IsFalse(result.Success);
        }
    }
}
