using System.Linq;
using System.Management.Automation;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Sets a CDS Environment Variables Value.</para>
    /// <para type="description">This cmdlet sets a CDS Environment Variable value
    /// using Environemnt Variables Definition SchemaName
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Set-XrmEnvironmentVariableValue -Name "new_Name" -Value "myvalue""</code>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "XrmEnvironmentVariableValue")]
    public class SetXrmEnvironmentVariableValueCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The SchemaName of the environment variable definition</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The Value of the Environment Variable</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Value { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            EnvironmentVariablesManager manager = new EnvironmentVariablesManager(Logger, OrganizationService);

            manager.SetValue(Name, Value);
        }

        #endregion
    }
}
