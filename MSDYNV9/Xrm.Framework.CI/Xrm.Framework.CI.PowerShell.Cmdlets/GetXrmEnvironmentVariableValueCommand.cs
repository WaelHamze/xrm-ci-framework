using System.Linq;
using System.Management.Automation;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves a CDS Environment Variables Value.</para>
    /// <para type="description">This cmdlet retrieves a CDS Environment Variable value
    /// using Environemnt Variables Definition SchemaName
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Get-XrmEnvironmentVariableValue -Name "new_Name""</code>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "XrmEnvironmentVariableValue")]
    [OutputType(typeof(string))]
    public class GetXrmEnvironmentVariableValueCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The SchemaName of the environment variable definition</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            EnvironmentVariablesManager manager = new EnvironmentVariablesManager(Logger, OrganizationService);

            string value = manager.GetValue(Name);

            WriteObject(value);
        }

        #endregion
    }
}
