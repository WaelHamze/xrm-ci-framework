using System;
using System.IO;
using System.Management.Automation;
using System.Threading;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Upgrades a CRM holding solution</para>
    /// <para type="description">This cmdlet upgrades a CRM holding solution and return AsyncJobId for async upgrades
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Export-XrmSolution -ConnectionString "" -EntityName "account"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsData.Merge, "XrmSolution")]
    public class MergeXrmSolutionCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The unique name of the solution to be exported.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string UniqueSolutionName { get; set; }

        public MergeXrmSolutionCommand()
        {
        }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Upgrading Solution: {0}", UniqueSolutionName));

            var upgradeSolutionRequest = new DeleteAndPromoteRequest
            {
                UniqueName = UniqueSolutionName,
            };

            OrganizationService.Execute(upgradeSolutionRequest);

            base.WriteVerbose(string.Format("{0} Upgrade Completed", UniqueSolutionName));
        }

        #endregion
    }
}