using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Colne a CRM Solution.</para>
    /// <para type="description">The Merge-XrmSolutionPatches clone solution from solution.
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Merge-XrmSolutionPatches -ConnectionString "" -DisplayName "Solution display name" -ParentSolutionUniqueName "UniqueSolutionName" -VersionNumber "1.0.0.1"</code>
    /// </example>
    [Cmdlet(VerbsData.Merge, "XrmSolutionPatches")]
    public class MergeXrmSolutionPatches : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The display name of the cloned patched solution.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string DisplayName { get; set; }

        /// <summary>
        /// <para type="description">The unique name of the parent solution components to be colned.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string ParentSolutionUniqueName { get; set; }

        /// <summary>
        /// <para type="description">The version name of patch has to be greater than parent solution.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string VersionNumber { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            base.WriteVerbose("Executing CloneAsSolutionRequest");
            using (var context = new CIContext(OrganizationService))
            {
                var solution = (from sol in context.SolutionSet
                                where sol.UniqueName == ParentSolutionUniqueName
                                select new Solution { Version = sol.Version, FriendlyName = sol.FriendlyName }).FirstOrDefault();
                if (solution == null || string.IsNullOrEmpty(solution.Version))
                {
                    throw new Exception(string.Format("Parent solution with unique name {0} not found.", ParentSolutionUniqueName));
                }

                if (string.IsNullOrEmpty(VersionNumber))
                {
                    string[] versions = solution.Version.Split('.');
                    char dot = '.';
                    VersionNumber = string.Concat(versions[0], dot, Convert.ToInt32(versions[1]) + 1, dot, versions[2], dot, versions[3]);
                    base.WriteVerbose(string.Format("New version number {0}", VersionNumber));
                }

                if (string.IsNullOrEmpty(DisplayName))
                {
                    DisplayName = solution.FriendlyName;
                }

                var cloneAsPatch = new CloneAsSolutionRequest
                {
                    DisplayName = DisplayName,
                    ParentSolutionUniqueName = ParentSolutionUniqueName,
                    VersionNumber = VersionNumber,
                };

                OrganizationService.Execute(cloneAsPatch);
            }

            base.WriteVerbose("Completed CloneAsSolutionRequest");
        }

        #endregion
    }
}