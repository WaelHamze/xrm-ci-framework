using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xrm.Framework.CI.Common.IntegrationTests.Logging;

namespace Xrm.Framework.CI.Common.IntegrationTests
{
    [TestClass]
    public class ConfigurationMigrationTest
    {
        #region Properties
        public TestContext TestContext
        {
            get;
            set;
        }

        public TestLogger Logger
        {
            get;
            set;
        }

        public System.Reflection.Assembly AssemblyInfo
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
            Logger = new TestLogger();
            ArtifactsDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Artifacts";
            AssemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
        }

        [TestMethod]
        public void TestSplitData()
        {
            string dataZip = Path.Combine(ArtifactsDirectory, "ExtractedPortalData.zip");
            
            string folder = Path.Combine(Path.GetDirectoryName(AssemblyInfo.Location), "temp", MethodBase.GetCurrentMethod().Name);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            TestLogger logger = new TestLogger();
            ConfigurationMigrationManager manager = new ConfigurationMigrationManager(logger);

            manager.ExpandData(dataZip, folder);

            manager.SplitData(folder);
        }

        [TestMethod]
        public void TestSplitDataFileLevel()
        {
            string dataZip = Path.Combine(ArtifactsDirectory, "ExtractedPortalData.zip");

            string folder = Path.Combine(Path.GetDirectoryName(AssemblyInfo.Location), "temp", MethodBase.GetCurrentMethod().Name);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            TestLogger logger = new TestLogger();
            ConfigurationMigrationManager manager = new ConfigurationMigrationManager(logger);

            manager.ExpandData(dataZip, folder);

            manager.SplitData(folder, CmExpandTypeEnum.RecordLevel);
        }

        [TestMethod]
        public void TestCombineData()
        {
            string existingDataZip = Path.Combine(ArtifactsDirectory, "ExtractedPortalData.zip");

            string tempFolder = Path.Combine(Path.GetDirectoryName(AssemblyInfo.Location), "temp", MethodBase.GetCurrentMethod().Name);
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            string recombinedDataZip = Path.Combine(tempFolder, "recombined-data.zip");

            TestLogger logger = new TestLogger();
            ConfigurationMigrationManager manager = new ConfigurationMigrationManager(logger);

            manager.ExpandData(existingDataZip, tempFolder);

            manager.SplitData(tempFolder);
            string combined = manager.CombineData(tempFolder);

            manager.CompressData(combined, recombinedDataZip);
        }

        [TestMethod]
        public void TestCombineDataFileLevel()
        {
            string existingDataZip = Path.Combine(ArtifactsDirectory, "ExtractedPortalData.zip");

            string tempFolder = Path.Combine(Path.GetDirectoryName(AssemblyInfo.Location), "temp", MethodBase.GetCurrentMethod().Name);
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            string recombinedDataZip = Path.Combine(tempFolder, "recombined-data.zip");

            TestLogger logger = new TestLogger();
            ConfigurationMigrationManager manager = new ConfigurationMigrationManager(logger);

            manager.ExpandData(existingDataZip, tempFolder);

            manager.SplitData(tempFolder, CmExpandTypeEnum.RecordLevel);
            string combined = manager.CombineData(tempFolder, CmExpandTypeEnum.RecordLevel);

            manager.CompressData(combined, recombinedDataZip);
        }

        [TestMethod]
        public void TestSort()
        {
            string folder = @"C:\Src\dyn365-ce-devops-sample\Sample\Xrm.CI.Framework.Sample\Data\Data";

            TestLogger logger = new TestLogger();
            ConfigurationMigrationManager manager = new ConfigurationMigrationManager(logger);

            manager.SortDataXml(folder);
        }
    }
}
