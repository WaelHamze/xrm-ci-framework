using System.Linq;
using System.Management.Automation;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.Common;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves a CRM Solution.</para>
    /// <para type="description">This cmdlet retrieves a CRM solution by unique name
    ///  and returns the strongly typed solution Entity object.
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Get-XrmSolution -ConnectionString "" -EntityName "account"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "XrmSolutionPatches")]
    [OutputType(typeof(Solution))]
    public class GetXrmSolutionPatchesCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The unique name of the solution to be retrieved.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string UniqueSolutionName { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Logger.LogVerbose("Entering XrmSolutionPatches");

            XrmConnectionManager xrmConnection = new XrmConnectionManager(
                Logger);

            IOrganizationService pollingOrganizationService = xrmConnection.Connect(
                ConnectionString,
                120);

            SolutionManager solutionManager = new SolutionManager(
                Logger,
                OrganizationService,
                pollingOrganizationService);

            List<Solution> patches = solutionManager.GetSolutionPatches(UniqueSolutionName);

            Logger.LogInformation("{0} patches found for solution {1}", patches.Count, UniqueSolutionName);

            base.WriteObject(patches);

            Logger.LogVerbose("Entering XrmSolutionPatches");
        }

        #endregion
    }
}
