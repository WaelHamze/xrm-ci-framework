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
    /// <para type="synopsis">Removes a CRM Solution Components.</para>
    /// <para type="description">The Remove-XrmSolutionComponents of a CRM solution by unique name.
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Remove-XrmSolutionComponents -ConnectionString "" -UniqueSolutionName "UniqueSolutionName"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "XrmSolutionComponents")]
    [OutputType(typeof(String))]
    public class RemoveXrmSolutionComponentsCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The unique name of the solution components to be removed.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string SolutionName { get; set; }
        
        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Removing Solution Components: {0}", SolutionName));

            using (var context = new CIContext(OrganizationService))
            {
                var query1 = from solution in context.SolutionSet
                            where solution.UniqueName == SolutionName
                            select solution.Id;

                if (query1 == null)
                {
                    throw new Exception(string.Format("Solution {0} could not be found", SolutionName));
                }

                var solutionId = query1.FirstOrDefault();
                
                var query = from s in context.SolutionComponentSet
                            where s.SolutionId == new EntityReference(Solution.EntityLogicalName, solutionId) && s.RootSolutionComponentId == null
                            select new { s.ComponentType, s.ObjectId};

                foreach (var solutionComponent in query.ToList())
                {
                    var removeReq = new RemoveSolutionComponentRequest()
                    {
                        ComponentId = (Guid)solutionComponent.ObjectId,
                        ComponentType = (int)solutionComponent.ComponentType.Value,
                        SolutionUniqueName = SolutionName
                    };
                    OrganizationService.Execute(removeReq);
                    base.WriteVerbose(string.Format("Removed component from solution with Id : {0} and Type: {1}", solutionComponent.ObjectId, solutionComponent.ComponentType.Value));
                }
            }
        }

        #endregion
    } 
}