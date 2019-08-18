using System;
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
            string dataZip = @"C:\Src\dyn365-ce-devops-sample\Sample\Xrm.CI.Framework.Sample\Data\export.zip";
            string folder = @"C:\Src\dyn365-ce-devops-sample\Sample\Xrm.CI.Framework.Sample\Data\Data";

            TestLogger logger = new TestLogger();
            ConfigurationMigrationManager manager = new ConfigurationMigrationManager(logger);

            manager.ExpandData(dataZip, folder);

            manager.SplitData(folder);
        }

        [TestMethod]
        public void TestCombineData()
        {
            string dataZip = @"C:\Src\dyn365-ce-devops-sample\Sample\Xrm.CI.Framework.Sample\Data\export_packed.zip";
            string folder = @"C:\Src\dyn365-ce-devops-sample\Sample\Xrm.CI.Framework.Sample\Data\Data";

            TestLogger logger = new TestLogger();
            ConfigurationMigrationManager manager = new ConfigurationMigrationManager(logger);

            manager.CombineData(folder);

            manager.CompressData(folder, dataZip);
        }

        [TestMethod]
        public void TestSort()
        {
            string folder = @"C:\Src\dyn365-ce-devops-sample\Sample\Xrm.CI.Framework.Sample\Data";

            TestLogger logger = new TestLogger();
            ConfigurationMigrationManager manager = new ConfigurationMigrationManager(logger);

            manager.SortDataXml(folder);
        }
    }
}
