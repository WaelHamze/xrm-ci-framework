using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.IntegrationTests.Logging;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common.IntegrationTests
{
    [TestClass]
    public class SolutionPackagerTest
    {

        public ILogger Logger
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

        [TestMethod]
        public void TestPackSolution()
        {
            Logger = new TestLogger();
            LogsDirectory = TestContext.TestLogsDir;
            LogFileName = $"{TestContext.TestName}.txt";

            string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            //TODO: Need to fix paths

            string solutionPackager = $"{currentDirectory}\\..\\..\\..\\packages\\Microsoft.CrmSdk.CoreTools.9.0.2.11\\content\\bin\\coretools\\SolutionPackager.exe";

            string folder = @"C:\Dev\Src\dyn365-ce-devops-sample\Sample\Xrm.CI.Framework.Sample\Customizations\Solutions\xRMCISample\Customizations";

            string mapping = @"C:\Dev\Src\dyn365-ce-devops-sample\Sample\Xrm.CI.Framework.Sample\Customizations\Solutions\xRMCISample\mapping.xml";

            SolutionPackager packager = new SolutionPackager(
                Logger,
                solutionPackager,
                LogsDirectory + "\\zzz.zip",
                folder,
                LogsDirectory + "\\" + LogFileName);

            bool success = packager.Pack(SolutionPackager_PackageType.Both, mapping, "", false,false);

            Assert.AreEqual(success, true);
        }

        [TestMethod()]
        public void TestPackSolutions()
        {
            Logger = new TestLogger();
            LogsDirectory = TestContext.TestLogsDir;
            LogFileName = $"{TestContext.TestName}.txt";

            string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            //TODO: Need to fix paths

            string solutionPackager = $"{currentDirectory}\\..\\..\\..\\packages\\Microsoft.CrmSdk.CoreTools.9.0.2.6\\content\\bin\\coretools\\SolutionPackager.exe";

            string folder = @"..\..\..\..\..\..\dyn365-ce-devops-sample\Sample\Xrm.CI.Framework.Sample\Customizations\Solutions\xRMCISample\Customizations";

            string mapping = @"..\..\..\..\..\..\dyn365-ce-devops-sample\Sample\Xrm.CI.Framework.Sample\Customizations\Solutions\xRMCISample\mapping.xml";

            SolutionPackConfig config = new SolutionPackConfig();
            config.Solutions.Add(new SolutionPackOptions()
            {
                Folder = folder,
                MappingFile = mapping,
                IncludeVersionInName = true,
                PackageType = SolutionPackager_PackageType.Both,
                TreatWarningsAsErrors = false,
                IncrementReleaseVersion = true,
                Version = string.Empty
                }
            );

            string configFile = $"{currentDirectory}\\PackConfig.json";

            Serializers.SaveJson<SolutionPackConfig>(configFile, config);

            SolutionPackagerManager packagerManager = new SolutionPackagerManager(Logger);

            List<bool> results = packagerManager.PackSolutions(solutionPackager, LogsDirectory, configFile, LogsDirectory);
        }

        [TestMethod]
        public void TestExtract()
        {
            Logger = new TestLogger();
            LogsDirectory = TestContext.TestLogsDir;
            LogFileName = $"{TestContext.TestName}.txt";

            string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            //TODO: Need to fix paths

            string solutionPackager = $"{currentDirectory}\\..\\..\\..\\packages\\Microsoft.CrmSdk.CoreTools.9.0.2.6\\content\\bin\\coretools\\SolutionPackager.exe";

            string extractDir = LogsDirectory + "Customizations";

            Directory.CreateDirectory(extractDir);

            SolutionPackager packager = new SolutionPackager(
                Logger,
                solutionPackager,
                currentDirectory + "\\..\\..\\Artifacts\\Success_1_0_0_0.zip",
                extractDir,
                LogsDirectory + "\\" + LogFileName);

            bool success = packager.Extract(
                SolutionPackager_PackageType.Both,
                string.Empty,
                true,
                true,
                true,
                string.Empty,
                false,
                false);

            Assert.AreEqual(success, true);
        }
    }
}
