using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Copies a CRM Solution as a patch.</para>
    /// <para type="description">The Add-XrmSolutionPatch create the empty clone solution.
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Add-XrmSolutionPatch -ConnectionString "" -DisplayName "Patched solution display name" -ParentSolutionUniqueName "UniqueSolutionName" -VersionNumber "1.0.0.1"</code>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "XrmSolutionPatch")]
    public class AddXrmSolutionPatch : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The display name of the cloned patched solution.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
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

            base.WriteVerbose("Executing CloneAsPatchRequest");
            using (var context = new CIContext(OrganizationService))
            {
                if (string.IsNullOrEmpty(VersionNumber))
                {
                    var solution = (from sol in context.SolutionSet
                                    where sol.UniqueName==ParentSolutionUniqueName || sol.UniqueName.StartsWith(ParentSolutionUniqueName + "_Patch")
                                    orderby sol.Version descending
                                    select new Solution { Version = sol.Version }).FirstOrDefault();
                    if (solution == null || string.IsNullOrEmpty(solution.Version))
                    {
                        throw new Exception(string.Format("Parent solution with unique name {0} not found.", ParentSolutionUniqueName));
                    }

                    string[] versions = solution.Version.Split('.');
                    char dot = '.';
                    VersionNumber = string.Concat(versions[0], dot, versions[1], dot, Convert.ToInt32(versions[2]) + 1, dot, versions[3]);
                    base.WriteVerbose(string.Format("New version number {0}", VersionNumber));
                }

                var cloneAsPatch = new CloneAsPatchRequest
                {
                    DisplayName = DisplayName,
                    ParentSolutionUniqueName = ParentSolutionUniqueName,
                    VersionNumber = VersionNumber,
                };

                OrganizationService.Execute(cloneAsPatch);
            }

            base.WriteVerbose("Completed CloneAsPatchRequest");
        }

        #endregion
    }
}