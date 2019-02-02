using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public class XrmConnectionConfigManager
    {
        #region Properties

        protected ILogger Logger
        {
            get;
            set;
        }

        protected IXrmEncryption Encryption
        {
            get;
            set;
        }

        public string ConfigPath
        {
            get;
            private set;
        }

        internal XrmConnectionInfoList ConnectionList
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public XrmConnectionConfigManager(ILogger logger, IXrmEncryption encryption, string configPath)
        {
            Logger = logger;
            Encryption = encryption;

            if (!String.IsNullOrEmpty(configPath))
            {
                ConfigPath = configPath;
            }
            else
            {
                ConfigPath = GetTempConfig();
            }
            Init();
        }

        #endregion

        #region Methods

        public void SetConnection(string key, string connectionString)
        {
            string encryptedValue = Encryption.Encrypt(connectionString);

            XrmConnectionInfo info = FindConnection(key);

            if (info != null)
            {
                info.Value = encryptedValue;
            }
            else
            {
                ConnectionList.Connections.Add(new XrmConnectionInfo
                {
                    Key = key,
                    Value = encryptedValue
                });
            }

            SaveList();
        }

        public void RemoveConnection(string key)
        {
            XrmConnectionInfo info = FindConnection(key);

            if (info != null)
            {
                ConnectionList.Connections.Remove(info);
                SaveList();
            }
            else
            {
                throw new Exception($"No connection found with key: {key}");
            }
        }

        public string GetConnection(string key)
        {
            XrmConnectionInfo found = FindConnection(key);

            if (found != null)
            {
                return Encryption.Decrypt(found.Value);
            }
            else
            {
                return null;
            }
        }

        public List<string> GetConnections()
        {
            return (from c in ConnectionList.Connections
                    select c.Key).ToList<string>();

        }

        private string GetTempConfig()
        {
            string tempFolder = Path.GetTempPath();
            string configFolder = Path.Combine(tempFolder, "xRMCIFramework");
            if (!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
            }
            string configFile = "connections.json";

            return Path.Combine(configFolder, configFile);
        }

        private void Init()
        {
            if (!File.Exists(ConfigPath))
            {
                ConnectionList = new XrmConnectionInfoList();
                SaveList();
            }
            else
            {
                LoadList();
            }
        }

        private XrmConnectionInfo FindConnection(string key)
        {
            var c = from cons in ConnectionList.Connections
                    where cons.Key == key
                    select cons;

            List<XrmConnectionInfo> found = c.ToList<XrmConnectionInfo>();

            if (found.Count > 1)
            {
                throw new Exception($"More than one connection found with key: {key}");
            }
            else if (found.Count == 1)
            {
                return found[0];
            }
            else
            {
                return null;
            }
        }

        private void SaveList()
        {
            Serializers.SaveJson<XrmConnectionInfoList>(ConfigPath, ConnectionList);
        }

        private void LoadList()
        {
            ConnectionList = Serializers.ParseJson<XrmConnectionInfoList>(ConfigPath);
        }

        #endregion
    }

    public class XrmConnectionInfo
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    internal class XrmConnectionInfoList
    {
        public List<XrmConnectionInfo> Connections { get; set; }

        public XrmConnectionInfoList()
        {
            Connections = new List<XrmConnectionInfo>();
        }
    }
}
