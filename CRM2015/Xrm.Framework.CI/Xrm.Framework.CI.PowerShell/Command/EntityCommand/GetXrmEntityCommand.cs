using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Xrm.Framework.CI.PowerShell.Command
{
    /// <summary>
    /// <para type="synopsis">Retrieves an entity record by id.</para>
    /// <para type="description">The Get-XrmEntity cmdlet retrieves an record of an entity by its id.
    /// The returned entity object will include all collumns of this record.</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Get-XrmEntity -EntityName "account" -Id "27c9fb86-1118-4f35-a3dc-6f0b1b344a9b"</code>
    ///   <para>Retrieves a record of the account entity by its id.</para>
    /// </example>
    /// <para type="link" uri="http://msdn.microsoft.com/en-us/library/microsoft.xrm.sdk.messages.retrieverequest.aspx">RetrieveRequest.</para>    
    [Cmdlet(VerbsCommon.Get, "XrmEntity")]
    [OutputType(typeof(Entity))]
    public class GetXrmEntityCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The logical name of the entity that is specified in the Id parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string EntityName { get; set; }

        /// <summary>
        /// <para type="description">The ID of the record that you want to retrieve.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public Guid Id { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Retrieving Entity: {0} {1}", EntityName, Id));

            Entity entity = OrganizationService.Retrieve(EntityName, Id, new ColumnSet(true));

            base.WriteVerbose(string.Format("Entity Retrieved: {0} {1}", entity.LogicalName, entity.Id));

            WriteObject(entity);
        }

        #endregion
    }
}