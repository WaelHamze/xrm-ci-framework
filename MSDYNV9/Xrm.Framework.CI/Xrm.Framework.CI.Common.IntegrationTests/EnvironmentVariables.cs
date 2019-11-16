using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common.IntegrationTests
{
    [TestClass]
    public class EnvironmentVariables
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

        public EnvironmentVariablesManager EnvironmentVariablesManager
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
            EnvironmentVariablesManager = new EnvironmentVariablesManager(Logger, OrganizationService);
        }

        [TestMethod]
        public void TestExport_Config()
        {
            string name = "ud_SampleText";
            string newValue = Guid.NewGuid().ToString();

            string value = EnvironmentVariablesManager.GetValue(name);

            EnvironmentVariablesManager.SetValue(name, newValue);

            value = EnvironmentVariablesManager.GetValue(name);

            Assert.AreEqual(value, newValue);

            EnvironmentVariablesManager.DeleteValue(name);

            value = EnvironmentVariablesManager.GetValue(name);

            Assert.AreEqual(value, null);

            EnvironmentVariablesManager.SetValue(name, newValue);

            value = EnvironmentVariablesManager.GetValue(name);

            Assert.AreEqual(value, newValue);
        }
    }
}
