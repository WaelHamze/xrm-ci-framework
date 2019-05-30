using System.Linq;
using System.Management.Automation;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

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
    [Cmdlet(VerbsCommon.Get, "XrmSolution")]
    [OutputType(typeof(Solution))]
    public class GetXrmSolutionCommand : XrmCommandBase
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

            base.WriteVerbose(string.Format("Retrieving Solution: {0}", UniqueSolutionName));

            using (var context = new CIContext(OrganizationService))
            {
                var query = from s in context.SolutionSet
                            where s.UniqueName == UniqueSolutionName
                            select s;

                Solution solution = query.FirstOrDefault();

                WriteObject(solution);
            }
        }

        #endregion
    }
}
