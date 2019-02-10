using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xrm.Framework.CI.Common.IntegrationTests.Logging;

namespace Xrm.Framework.CI.Common.IntegrationTests
{
    [TestClass]
    public class ConnectionConfigManagerTest
    {
        public TestContext TestContext
        {
            get;
            set;
        }

        [TestMethod]
        public void TestConnection()
        {
            string config = $"{TestContext.TestLogsDir}\\connections.json";

            TestLogger logger = new TestLogger();
            XrmEncryptionManager encryption = new XrmEncryptionManager(logger);
            XrmConnectionConfigManager manager = new XrmConnectionConfigManager(logger, encryption, config);

            string con1 = "AuthType=Office365;Username=user1@name.com;Password=passwork;Url=https://name1.crmregion.dynamics.com";
            string key1 = "crm1";

            string con2 = "AuthType=Office365;Username=user2@name.com;Password=passwork;Url=https://name2.crmregion.dynamics.com";
            string key2 = "crm2";

            Assert.AreEqual(manager.GetConnections().Count, 0);

            manager.SetConnection(key1, con1);

            Assert.AreEqual(con1, manager.GetConnection(key1));

            manager.SetConnection(key2, con2);

            Assert.AreEqual(con2, manager.GetConnection(key2));
            Assert.AreEqual(manager.GetConnections().Count, 2);
            Assert.AreEqual(manager.GetConnections()[0], key1);
            Assert.AreEqual(manager.GetConnections()[1], key2);

            manager.RemoveConnection(key1);

            Assert.AreEqual(manager.GetConnections().Count, 1);
            Assert.AreEqual(null, manager.GetConnection(key1));
            Assert.AreEqual(con2, manager.GetConnection(key2));

            manager.RemoveConnection(key2);

            Assert.AreEqual(null, manager.GetConnection(key2));
            Assert.AreEqual(manager.GetConnections().Count, 0);
        }
    }
}
