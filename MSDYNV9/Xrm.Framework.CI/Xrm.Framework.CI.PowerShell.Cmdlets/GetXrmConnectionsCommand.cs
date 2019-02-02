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
    [Cmdlet(VerbsCommon.Get, "XrmConnections")]
    [OutputType(typeof(string[]))]
    public class GetXrmConnectionsCommand : CommandBase
    {
        #region Parameters

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

            Logger.LogInformation("Retrieving connections");

            XrmEncryptionManager encryption = new XrmEncryptionManager(Logger);
            XrmConnectionConfigManager manager = new XrmConnectionConfigManager(Logger, encryption, ConfigFilePath);

            List<string> connecitons = manager.GetConnections();

            base.WriteObject(manager.GetConnections().ToArray());

            Logger.LogInformation("Retrieved {0} connection(s)from {1}", connecitons.Count, manager.ConfigPath);
        }

        #endregion
    }
}