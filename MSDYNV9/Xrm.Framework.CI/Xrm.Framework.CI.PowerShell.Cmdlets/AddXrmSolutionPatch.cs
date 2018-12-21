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

            base.WriteVerbose("Executing CloneAsPatchRequest");

            using (var context = new CIContext(OrganizationService))
            {
                base.WriteVerbose("VersionNumber not supplied. Generating default VersionNumber");

                if (string.IsNullOrEmpty(VersionNumber))
                {
                    var solution = (from sol in context.SolutionSet
                                    where sol.UniqueName==ParentSolutionUniqueName || sol.UniqueName.StartsWith(ParentSolutionUniqueName + "_Patch")
                                    orderby sol.Version descending
                                    select new Solution { Version = sol.Version, FriendlyName = sol.FriendlyName }).FirstOrDefault();
                    if (solution == null || string.IsNullOrEmpty(solution.Version))
                    {
                        throw new Exception(string.Format("Parent solution with unique name {0} not found.", ParentSolutionUniqueName));
                    }

                    string[] versions = solution.Version.Split('.');
                    char dot = '.';
                    VersionNumber = string.Concat(versions[0], dot, versions[1], dot, Convert.ToInt32(versions[2]) + 1, dot, 0);
                    base.WriteVerbose(string.Format("New VersionNumber: {0}", VersionNumber));
                }

                if (string.IsNullOrEmpty(DisplayName))
                {
                    var solution = (from sol in context.SolutionSet
                                    where sol.UniqueName == ParentSolutionUniqueName
                                    select new Solution { FriendlyName = sol.FriendlyName }).FirstOrDefault();
                    base.WriteVerbose((solution == null).ToString());
                    base.WriteVerbose(solution.FriendlyName);

                    if (solution == null || string.IsNullOrEmpty(solution.FriendlyName))
                    {
                        throw new Exception(string.Format("Parent solution with unique name {0} not found.", ParentSolutionUniqueName));
                    }

                    DisplayName = solution.FriendlyName;
                }

                var cloneAsPatch = new CloneAsPatchRequest
                {
                    DisplayName = DisplayName,
                    ParentSolutionUniqueName = ParentSolutionUniqueName,
                    VersionNumber = VersionNumber,
                };

                CloneAsPatchResponse response  = OrganizationService.Execute(cloneAsPatch) as CloneAsPatchResponse;

                base.WriteVerbose(string.Format("Patch solution created with Id {0}", response.SolutionId));

                base.WriteVerbose("Retrieving Patch Name");

                var patch = (from sol in context.SolutionSet
                                where sol.Id == response.SolutionId
                                select new Solution { UniqueName = sol.UniqueName }).FirstOrDefault();
                if (patch == null || string.IsNullOrEmpty(patch.UniqueName))
                {
                    throw new Exception(string.Format("Solution with Id {0} not found.", response.SolutionId));
                }

                base.WriteVerbose(string.Format("Patch solution name: {0}", patch.UniqueName));

                base.WriteObject(patch.UniqueName);
            }

            base.WriteVerbose("Completed CloneAsPatchRequest");
        }

        #endregion
    }
}