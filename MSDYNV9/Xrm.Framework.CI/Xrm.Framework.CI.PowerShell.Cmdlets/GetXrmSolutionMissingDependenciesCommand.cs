using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System.Linq;
using System.Management.Automation;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves missing dependencies for a solution in a CRM instance.</para>
    /// <para type="description">This cmdlet retrieves missing depeendencies for a given solution
    /// in the source CRM instance
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Get-XrmSolutionMissingComponents -ConnectionString "" -SolutionFilePath "C:\solution.zip"</code>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "XrmSolutionMissingDependencies")]
    [OutputType(typeof(Solution))]
    public class GetXrmSolutionMissingDependencies : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The unique name of the solution to be checked.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string UniqueSolutionName { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            Logger.LogVerbose("Entering Get-XrmSolutionMissingDependencies");

            base.ProcessRecord();

            Logger.LogVerbose("Retrieving missing dependencies for {0}", UniqueSolutionName);

            SolutionComponentsManager manager = new SolutionComponentsManager(Logger, OrganizationService);

            EntityCollection dependencies = manager.GetMissingDependencies(UniqueSolutionName);

            WriteObject(dependencies);

            Logger.LogVerbose("Leaving Get-XrmSolutionMissingDependencies");
        }

        #endregion
    }
}
