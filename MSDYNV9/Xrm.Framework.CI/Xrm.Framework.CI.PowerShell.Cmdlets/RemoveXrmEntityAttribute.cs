using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Deletes an entity attribute.</para>
    /// <para type="description">The Remove-XrmEntity cmdlet deletes an attribute from an entity.</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Remove-XrmEntityAttribute -EntityName "new_demoentity" -AttributeName "new_removeme"</code>
    ///   <para>Removes the new_removeme attribute from the new_demoentity entity</para>
    /// </example>
    /// <para type="link" uri="http://msdn.microsoft.com/en-us/library/microsoft.xrm.sdk.messages.deleteattributerequest.aspx">DeleteAttributeRequest.</para>
    [Cmdlet(VerbsCommon.Remove, "XrmEntityAttribute")]
    public class RemoveXrmEntityAttributeCommand: XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The logical name of the entity that is specified in the Id parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string EntityName { get; set; }

        /// <summary>
        /// <para type="description">The logical name of the entity that is specified in the Id parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string AttributeName { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Deleting Entity: {0}", EntityName));
            Microsoft.Xrm.Sdk.Messages.DeleteAttributeRequest deleterequest = new Microsoft.Xrm.Sdk.Messages.DeleteAttributeRequest { EntityLogicalName = EntityName, LogicalName = AttributeName };
            OrganizationService.Execute(deleterequest);

            base.WriteVerbose(string.Format("Entity Deleted: {0}", EntityName));
        }

        #endregion
    }
}