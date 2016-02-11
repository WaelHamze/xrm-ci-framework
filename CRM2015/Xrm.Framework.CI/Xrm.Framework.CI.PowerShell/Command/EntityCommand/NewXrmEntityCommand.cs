using System.Management.Automation;
using Microsoft.Xrm.Sdk;

namespace Xrm.Framework.CI.PowerShell.Command
{
    /// <summary>
    /// <para type="synopsis">Creates a new entity object from a specified type.</para>
    /// <para type="description">The New-XrmEntity cmdlet creates a new empty entity object from a given type.
    /// You can use this object with the "Add-XrmEntity" and "Set-XrmEntity" cmdlet.</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>New-XrmEntity -EntityName "account"</code>
    ///   <para>Create a new entity object from type "account".</para>
    /// </example>
    /// <para type="link">New-XrmEntity</para>
    /// <para type="link">Set-XrmEntity</para>
    /// <para type="link" uri="http://msdn.microsoft.com/en-us/library/microsoft.xrm.sdk.entity.aspx">Entity</para>
    [Cmdlet(VerbsCommon.New, "XrmEntity")]
    [OutputType(typeof(Entity))]
    public class NewXrmEntityCommand : Cmdlet
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The logical name of the entity for which a new record (instance) should be created.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string EntityName { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("New Entity: {0}", EntityName));

            WriteObject(new Entity(EntityName));
        }

        #endregion
    }
}