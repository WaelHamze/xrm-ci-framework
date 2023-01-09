using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Creates a record of an entity.</para>
    /// <para type="description">The Add-XrmEntity cmdlet creates a record of any entity that supports the Create message, including custom entities.
    ///  The property "LogicalName" on the entity object is mandatory.</para>
    /// <para type="description">Executes a "CreateRequest".</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>$entityObject = New-XrmEntity -EntityName "account"</code>
    ///   <code>C:\PS>Create-XrmEntity -EntityObject $entityObject</code>
    ///   <para>Creates a record of the account entity.</para>
    /// </example>
    /// <para type="link">New-XrmEntity</para>
    /// <para type="link" uri="http://msdn.microsoft.com/en-us/library/microsoft.xrm.sdk.messages.createrequest.aspx">CreateRequest.</para>
    [Cmdlet(VerbsCommon.Add, "XrmEntity")]
    [OutputType(typeof(Guid))]
    public class AddXrmEntityCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">An entity instance that contains the properties to set in the newly created record. 
        /// Use the "New-XrmEntity" cmdlet to create an new Entity instance.
        /// The "LogicalName" property on the entity object is mandatory.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public Entity EntityObject { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose(string.Format("Creating Entity: {0}", EntityObject));

            Guid id = OrganizationService.Create(EntityObject);
            WriteObject(id);

            WriteVerbose(string.Format("Entity Created: {0}", EntityObject));
        }

        #endregion
    }
}