using System;
using System.IO;
using System.Reflection;
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

        #endregion

        [TestInitialize()]
        public void Setup()
        {
            Logger = new TestLogger();
            OrganizationService = new XrmConnectionManager().CreateConnection();
            ArtifactsDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Artifacts";
            LogsDirectory = TestContext.TestLogsDir;
            LogFileName = $"{TestContext.TestName}.xml";
        }

        [TestMethod]
        public void ImportSolution_Async_Success()
        {
            SolutionManager solutionManager = 
                new SolutionManager(Logger, OrganizationService);

            solutionManager.DeleteSolution("Success");

            SolutionImportResult result = solutionManager.ImportSolution(
                $"{ArtifactsDirectory}\\Success_1_0_0_0.zip",
                true,
                false,
                true,
                false,
                false,
                false,
                true,
                1,
                300,
                Guid.NewGuid(),
                true,
                LogsDirectory,
                LogFileName);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.IsTrue(result.UnprocessedComponents == 0);
            Assert.IsTrue(result.ImportJobAvailable);
            Assert.IsTrue(File.Exists($"{LogsDirectory}\\{LogFileName}"));

            result = solutionManager.ImportSolution(
                $"{ArtifactsDirectory}\\Success_1_0_0_0.zip",
                true,
                false,
                true,
                false,
                false,
                false,
                true,
                1,
                300,
                Guid.NewGuid(),
                true,
                LogsDirectory,
                LogFileName);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.ImportSkipped);
            Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.IsTrue(result.UnprocessedComponents == -1);
            Assert.IsFalse(result.ImportJobAvailable);
        }

        [TestMethod]
        public void ImportSolution_Async_Fail_MissingDependency()
        {
            SolutionManager solutionManager =
                new SolutionManager(Logger, OrganizationService);

            SolutionImportResult result = solutionManager.ImportSolution(
                $"{ArtifactsDirectory}\\MissingDependency_1_0_0_0_managed.zip",
                true,
                false,
                true,
                false,
                false,
                false,
                true,
                1,
                300,
                Guid.NewGuid(),
                true,
                LogsDirectory,
                LogFileName);

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("MissingDependency"));
            Assert.IsTrue(result.UnprocessedComponents > 0);
            Assert.IsTrue(result.ImportJobAvailable);
            Assert.IsTrue(File.Exists($"{LogsDirectory}\\{LogFileName}"));
        }

        [TestMethod]
        public void ImportSolution_Async_Fail_InvalidFile()
        {
            SolutionManager solutionManager =
                new SolutionManager(Logger, OrganizationService);

            SolutionImportResult result = solutionManager.ImportSolution(
                $"{ArtifactsDirectory}\\InvalidSolutionFile.txt",
                true,
                false,
                true,
                false,
                false,
                false,
                true,
                1,
                300,
                Guid.NewGuid(),
                true,
                LogsDirectory,
                LogFileName);

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("The solution file is invalid"));
            Assert.IsTrue(result.UnprocessedComponents == -1);
            Assert.IsFalse(result.ImportJobAvailable);
            Assert.IsFalse(File.Exists($"{LogsDirectory}\\{LogFileName}"));
        }

        [TestMethod]
        public void ImportSolution_Sync_Success()
        {
            SolutionManager solutionManager =
                new SolutionManager(Logger, OrganizationService);

            solutionManager.DeleteSolution("Success");

            SolutionImportResult result = solutionManager.ImportSolution(
                $"{ArtifactsDirectory}\\Success_1_0_0_0.zip",
                true,
                false,
                true,
                false,
                false,
                false,
                false,
                1,
                300,
                Guid.NewGuid(),
                true,
                LogsDirectory,
                LogFileName);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.IsTrue(result.UnprocessedComponents == 0);
            Assert.IsTrue(result.ImportJobAvailable);
            Assert.IsTrue(File.Exists($"{LogsDirectory}\\{LogFileName}"));
        }

        [TestMethod]
        public void ImportSolution_Sync_Fail_MissingDependency()
        {
            SolutionManager solutionManager =
                new SolutionManager(Logger, OrganizationService);

            SolutionImportResult result = solutionManager.ImportSolution(
                $"{ArtifactsDirectory}\\MissingDependency_1_0_0_0_managed.zip",
                true,
                false,
                true,
                false,
                false,
                false,
                false,
                1,
                300,
                Guid.NewGuid(),
                true,
                LogsDirectory,
                LogFileName);

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("MissingDependency"));
            Assert.IsTrue(result.UnprocessedComponents > 0);
            Assert.IsTrue(result.ImportJobAvailable);
            Assert.IsTrue(File.Exists($"{LogsDirectory}\\{LogFileName}"));
        }

        [TestMethod]
        public void ImportSolution_Sync_Fail_InvalidFile()
        {
            SolutionManager solutionManager =
                new SolutionManager(Logger, OrganizationService);

            SolutionImportResult result = solutionManager.ImportSolution(
                $"{ArtifactsDirectory}\\InvalidSolutionFile.txt",
                true,
                false,
                true,
                false,
                false,
                false,
                false,
                1,
                300,
                Guid.NewGuid(),
                true,
                LogsDirectory,
                LogFileName);

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("The solution file is invalid"));
            Assert.IsTrue(result.UnprocessedComponents == -1);
            Assert.IsFalse(result.ImportJobAvailable);
            Assert.IsFalse(File.Exists($"{LogsDirectory}\\{LogFileName}"));
        }
    }
}
