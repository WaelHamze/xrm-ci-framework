using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common.IntegrationTests
{
    [TestClass]
    public class ExportSolution
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

        public string LogsDirectory
        {
            get;
            set;
        }

        public SolutionManager SolutionManager
        {
            get;
            set;
        }

        public string ArtifactsDirectory
        {
            get;
            set;
        }

        #endregion

        [TestInitialize()]
        public void Setup()
        {
            Logger = new Logging.TestLogger();
            OrganizationService = new TestConnectionManager().CreateConnection();
            LogsDirectory = TestContext.TestLogsDir;
            SolutionManager = new SolutionManager(Logger, OrganizationService, null);
        }

        [TestMethod]
        public void TestExport_Config()
        {
            SolutionExportConfig config = new SolutionExportConfig();

            config.Solutions.Add(
                new SolutionExportOptions()
                {
                    SolutionName = "xRMCISample",
                    IncludeVersionInName = true,
                    Managed = true,
                    ExportAsync = true
                }
                );

            string configFile = $"{TestContext.TestLogsDir}\\ImportConfig.json";

            Serializers.SaveJson<SolutionExportConfig>(
                configFile,
                config);

            //var x = OrganizationService.Retrieve("exportsolutionupload", Guid.NewGuid(), new ColumnSet(true));

            List<string> exportedFiles =
                SolutionManager.ExportSolutions(LogsDirectory, config);

            Assert.IsTrue(File.Exists(exportedFiles[0]));
        }
    }
}
