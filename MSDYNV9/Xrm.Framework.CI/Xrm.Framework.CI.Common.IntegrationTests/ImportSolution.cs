using System;
using System.Collections.Generic;
using System.IO;
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
        public void ImportSolution_Async_Success()
        {
            SolutionManager.DeleteSolution("Success");

            SolutionImportResult result = SolutionManager.ImportSolution(
                $"{ArtifactsDirectory}\\Success_1_0_0_0_managed.zip",
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

            result = SolutionManager.ImportSolution(
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
        public void ImportSolution_Async_Success_Holding()
        {
            SolutionManager.DeleteSolution("Success_Upgrade");
            SolutionManager.DeleteSolution("Success");

            SolutionImportResult result = SolutionManager.ImportSolution(
                $"{ArtifactsDirectory}\\Success_1_0_0_0_managed.zip",
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

            result = SolutionManager.ImportSolution(
                $"{ArtifactsDirectory}\\Success_1_1_0_0_managed.zip",
                true,
                false,
                true,
                false,
                true,
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

            result = SolutionManager.ImportSolution(
                $"{ArtifactsDirectory}\\Success_1_1_0_0_managed.zip",
                true,
                false,
                true,
                false,
                true,
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
            SolutionImportResult result = SolutionManager.ImportSolution(
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
            SolutionImportResult result = SolutionManager.ImportSolution(
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
            SolutionManager.DeleteSolution("Success_Upgrade");
            SolutionManager.DeleteSolution("Success");

            SolutionImportResult result = SolutionManager.ImportSolution(
                $"{ArtifactsDirectory}\\Success_1_0_0_0_managed.zip",
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
        public void ImportSolution_Sync_Success_Holding()
        {
            SolutionManager.DeleteSolution("Success_Upgrade");
            SolutionManager.DeleteSolution("Success");

            SolutionImportResult result = SolutionManager.ImportSolution(
                $"{ArtifactsDirectory}\\Success_1_0_0_0_managed.zip",
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

            result = SolutionManager.ImportSolution(
                $"{ArtifactsDirectory}\\Success_1_1_0_0_managed.zip",
                true,
                false,
                true,
                false,
                true,
                false,
                false,
                1,
                300,
                Guid.NewGuid(),
                true,
                LogsDirectory,
                $"1_{LogFileName}");

            Assert.IsTrue(result.Success);
            Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.IsTrue(result.UnprocessedComponents == 0);
            Assert.IsTrue(result.ImportJobAvailable);
            Assert.IsTrue(File.Exists($"{LogsDirectory}\\1_{LogFileName}"));

            result = SolutionManager.ImportSolution(
                $"{ArtifactsDirectory}\\Success_1_1_0_0_managed.zip",
                true,
                false,
                true,
                false,
                true,
                false,
                false,
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
        public void ImportSolution_Sync_Fail_MissingDependency()
        {
            SolutionImportResult result = SolutionManager.ImportSolution(
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
            SolutionImportResult result = SolutionManager.ImportSolution(
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

        [TestMethod]
        public void ImportExport_E2E()
        {
            SolutionImportConfig importConfig = new SolutionImportConfig();

            importConfig.Solutions.Add(
                new SolutionImportOptions()
                {
                    SolutionFilePath = "Success_1_0_0_0_managed.zip",
                    ImportAsync = true,
                    OverrideSameVersion = true,
                    OverwriteUnmanagedCustomizations = true,
                    PublishWorkflows = true,
                });

            importConfig.Solutions.Add(
                new SolutionImportOptions()
                {
                    SolutionFilePath = "Success_1_1_0_0_managed.zip",
                    ImportAsync = true,
                    HoldingSolution = true,
                    ApplySolution = true,
                    ApplyAsync = true,
                    OverwriteUnmanagedCustomizations = true,
                    PublishWorkflows = true,
                });

            string importConfigFile = $"{ArtifactsDirectory}\\ImportConfig.json";

            Serializers.SaveJson<SolutionImportConfig>(
                importConfigFile,
                importConfig);

            List<SolutionImportResult> results = SolutionManager.ImportSolutions(
                ArtifactsDirectory,
                LogsDirectory,
                importConfig);

            Assert.IsTrue(results[0].Success);
            Assert.IsTrue(results[1].Success);

            //SolutionExportConfig config = new SolutionExportConfig();

            //config.Solutions.Add(
            //    new SolutionExportOptions()
            //    {
            //        SolutionName = "Success",
            //        IncludeVersionInName = true,
            //        Managed = true
            //    }
            //    );

            //string exportConfigFile = $"{TestContext.TestLogsDir}\\ExportConfig.json";

            //Serializers.SaveJson<SolutionExportConfig>(
            //    exportConfigFile,
            //    config);

            //List<string> exportedFiles =
            //    SolutionManager.ExportSolutions(LogsDirectory, config);

            //Assert.IsTrue(File.Exists(exportedFiles[0]));
        }
    }
}
