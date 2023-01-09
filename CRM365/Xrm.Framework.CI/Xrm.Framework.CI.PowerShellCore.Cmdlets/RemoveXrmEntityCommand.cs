using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Deletes an existing entity.</para>
    /// <para type="description">The Remove-XrmEntity cmdlet deletes an entity from an organization. Note, this does not automatically resolve any dependencies.</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Remove-XrmEntity -EntityName "new_demoentity"</code>
    ///   <para>Deletes the new_demoentity from the connected organization.</para>
    /// </example>
    /// <para type="link" uri="http://msdn.microsoft.com/en-us/library/microsoft.xrm.sdk.messages.deleteentityrequest.aspx">DeleteEntityRequest.</para>
    [Cmdlet(VerbsCommon.Remove, "XrmEntity")]
    public class RemoveXrmEntityCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The logical name of the entity to delete.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string EntityName { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Deleting Entity: {0}", EntityName));
            Microsoft.Xrm.Sdk.Messages.DeleteEntityRequest deleterequest = new Microsoft.Xrm.Sdk.Messages.DeleteEntityRequest { LogicalName = EntityName };
            OrganizationService.Execute(deleterequest);

            base.WriteVerbose(string.Format("Entity Deleted: {0}", EntityName));
        }

        #endregion
    }
}