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
        public TestContext TestContext
        {
            get;
            set;
        }

        [TestMethod]
        public void TestSplitData()
        {
            var assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
            string artifactsDirectory = Path.Combine(Path.GetDirectoryName(assemblyInfo.Location), "Artifacts");
            string dataZip = Path.Combine(artifactsDirectory, "ExtractedPortalData.zip");
            
            string folder = Path.Combine(Path.GetDirectoryName(assemblyInfo.Location), "temp", MethodBase.GetCurrentMethod().Name);
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
            var assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
            string artifactsDirectory = Path.Combine(Path.GetDirectoryName(assemblyInfo.Location), "Artifacts");
            string dataZip = Path.Combine(artifactsDirectory, "ExtractedPortalData.zip");

            string folder = Path.Combine(Path.GetDirectoryName(assemblyInfo.Location), "temp", MethodBase.GetCurrentMethod().Name);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            TestLogger logger = new TestLogger();
            ConfigurationMigrationManager manager = new ConfigurationMigrationManager(logger);

            manager.ExpandData(dataZip, folder);

            manager.SplitData(folder, CmExpandTypeEnum.FileLevel);
        }

        [TestMethod]
        public void TestCombineData()
        {
            string dataZip = @"C:\Temp\TestReferenceData\export_packed.zip";
            string folder = @"C:\Temp\TestReferenceData\Unpacked";

            TestLogger logger = new TestLogger();
            ConfigurationMigrationManager manager = new ConfigurationMigrationManager(logger);

            string combined = manager.CombineData(folder);

            manager.CompressData(combined, dataZip);
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
