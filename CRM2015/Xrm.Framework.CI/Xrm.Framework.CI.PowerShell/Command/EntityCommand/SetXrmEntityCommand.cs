using System.Management.Automation;
using Microsoft.Xrm.Sdk;

namespace Xrm.Framework.CI.PowerShell.Command
{
    /// <summary>
    /// <para type="synopsis">Updates an existing entity record.</para>
    /// <para type="description">The Set-XrmEntity cmdlet updates an record of an entity.
    /// All values from attributes which are attached to the entity object are updates also if they have not changed.
    /// The properties "LogicalName" and "Id" on the entity object are mandetory. Entity objects provided as output by the Cmdlets "New-XrmEntity", "Get-XrmEntity" and "Get-XrmEntities" can be used.</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Get-XrmEntity -EntityObject $entityObject</code>
    ///   <para>Updates a record.</para>
    /// </example>
    /// <para type="link">New-XrmEntity</para>
    /// <para type="link">Get-XrmEntity</para>
    /// <para type="link">Get-XrmEntities</para>
    /// <para type="link" uri="http://msdn.microsoft.com/en-us/library/microsoft.xrm.sdk.messages.updaterequest.aspx">UpdateRequest.</para>
    [Cmdlet(VerbsCommon.Set, "XrmEntity")]
    public class SetXrmEntityCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">An entity instance that has one or more properties set to be updated in the record. The properties "LogicalName" and "Id" on the entity object are mandetory.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public Entity EntityObject { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Updating Entity: {0}", EntityObject));

            OrganizationService.Update(EntityObject);

            base.WriteVerbose(string.Format("Entity Updated: {0}", EntityObject));
        }

        #endregion
    }
}