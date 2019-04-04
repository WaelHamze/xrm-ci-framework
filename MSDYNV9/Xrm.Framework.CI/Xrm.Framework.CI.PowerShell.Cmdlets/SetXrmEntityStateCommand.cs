using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
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
    [Cmdlet(VerbsCommon.Set, "XrmEntityState")]
    public class SetXrmEntityStateCommand : XrmCommandBase
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

        /// <summary>
        /// <para type="description">An entity instance that has one or more properties set to be updated in the record. The properties "LogicalName" and "Id" on the entity object are mandetory.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public int StateCode { get; set; }

        /// <summary>
        /// <para type="description">An entity instance that has one or more properties set to be updated in the record. The properties "LogicalName" and "Id" on the entity object are mandetory.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public int StatusCode { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Updating State: {0} {1}", StateCode, StatusCode));

            SetStateRequest request = new SetStateRequest()
            {
                EntityMoniker = new EntityReference(EntityName, Id),
                State = new OptionSetValue(StateCode),
                Status = new OptionSetValue(StatusCode)
            };

            OrganizationService.Execute(request);

            base.WriteVerbose(string.Format("Entity State Updated"));
        }

        #endregion
    }
}