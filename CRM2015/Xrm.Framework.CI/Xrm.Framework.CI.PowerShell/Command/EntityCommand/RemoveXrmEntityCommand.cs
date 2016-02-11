using System;
using System.Management.Automation;

namespace Xrm.Framework.CI.PowerShell.Command
{
    /// <summary>
    /// <para type="synopsis">Deletes an existing entity record.</para>
    /// <para type="description">The Remove-XrmEntity cmdlet deletes an record of an entity by its id.</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Delete-XrmEntity -EntityName "account" -Id "27c9fb86-1118-4f35-a3dc-6f0b1b344a9b"</code>
    ///   <para>Deletes a record of the account entity by its id.</para>
    /// </example>
    /// <para type="link" uri="http://msdn.microsoft.com/en-us/library/microsoft.xrm.sdk.messages.deleterequest.aspx">DeleteRequest.</para>
    [Cmdlet(VerbsCommon.Remove, "XrmEntity")]
    public class RemoveXrmEntityCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The logical name of the entity that is specified in the Id parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string EntityName { get; set; }

        /// <summary>
        /// <para type="description">The ID of the record that you want to delete.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public Guid Id { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Deleting Entity: {0} {1}", EntityName, Id));

            OrganizationService.Delete(EntityName, Id);

            base.WriteVerbose(string.Format("Entity Deleted: {0} {1}", EntityName, Id));
        }

        #endregion
    }
}