using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Xrm.Framework.CI.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Saves an encrypted connection string in config</para>
    /// <para type="description">This cmdlet can be used to test your connectivity to CRM by calling 
    /// WhoAmIRequest and returning a WhoAmIResponse object.
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Export-XrmSolution -ConnectionString "" -EntityName "account"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "XrmConnection")]
    public class RemoveXrmConnectionCommand : CommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The key for the connection string</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Key { get; set; }

        /// <summary>
        /// <para type="description">The absolute path to the json config file containing the connection. If not supplied connections.json will be created in user temp folder</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string ConfigFilePath { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Logger.LogInformation("Removing connection string with key: {0}", Key);

            XrmEncryptionManager encryption = new XrmEncryptionManager(Logger);
            XrmConnectionConfigManager manager = new XrmConnectionConfigManager(Logger, encryption, ConfigFilePath);

            manager.RemoveConnection(Key);

            Logger.LogInformation("Removing connection string with key: {0} from {1}", Key, manager.ConfigPath);
        }

        #endregion
    }
}