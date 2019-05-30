using System;
using System.Linq;
using System.Management.Automation;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Updates the version number of a CRM solution</para>
    /// <para type="description">This cmdlet updates the version number of a CRM solution using the solution unique name
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Export-XrmSolution -ConnectionString "" -EntityName "account"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "XrmSolutionVersion")]
    public class SetXrmSolutionVersionCommand : XrmCommandBase
    {
        #region Parameters
        /// <summary>
        /// <para type="description">The unique solution name to be updated</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string SolutionName { get; set; }

        /// <summary>
        /// <para type="description">The new version number of the solution</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Version { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Updating Solution {0} Version: {1}", SolutionName, Version));

            Solution solution;

            using (var context = new CIContext(OrganizationService))
            {
                var query = from s in context.SolutionSet
                            where s.UniqueName == SolutionName
                            select s;

                solution = query.FirstOrDefault();
            }

            if (solution == null)
            {
                throw new Exception(string.Format("Solution {0} could not be found", SolutionName));
            }

            var update = new Solution
            {
                Id = solution.Id,
                Version = Version
            };

            OrganizationService.Update(update);

            base.WriteVerbose(string.Format("Solution {0} Update to Version: {1}", SolutionName, Version));
        }

        #endregion
    }
}