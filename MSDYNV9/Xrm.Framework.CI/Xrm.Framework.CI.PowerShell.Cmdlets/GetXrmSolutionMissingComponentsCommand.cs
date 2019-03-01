using Microsoft.Crm.Sdk.Messages;
using System.Linq;
using System.Management.Automation;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves missing components for a solution on a target CRM instance.</para>
    /// <para type="description">This cmdlet retrieves missing solution components from a target instance by providing
    ///  the exported solution zip file from the source instance
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Get-XrmSolutionMissingComponents -ConnectionString "" -SolutionFilePath "C:\solution.zip"</code>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "XrmSolutionMissingComponents")]
    [OutputType(typeof(Solution))]
    public class GetXrmSolutionMissingComponents : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The absolute path to the solution file to be checked</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string SolutionFilePath { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            Logger.LogVerbose("Entering Get-XrmSolutionMissingComponents");

            base.ProcessRecord();

            Logger.LogVerbose("Retrieving missing components for {0}", SolutionFilePath);

            SolutionComponentsManager manager = new SolutionComponentsManager(Logger, OrganizationService);

            MissingComponent[] components = manager.GetMissingComponentsOnTarget(SolutionFilePath);

            WriteObject(components);

            Logger.LogVerbose("Leaving Get-XrmSolutionMissingComponents");
        }

        #endregion
    }
}
