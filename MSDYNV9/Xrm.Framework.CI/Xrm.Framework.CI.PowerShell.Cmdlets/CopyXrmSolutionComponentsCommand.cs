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
    /// <para type="synopsis">Copies a CRM Solution Components.</para>
    /// <para type="description">The Move-XrmSolutionComponents of a CRM solution to another by unique name.
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Move-XrmSolutionComponents -ConnectionString "" -FromSolutionName "UniqueSolutionName -ToSolutionName "UniqueSolutionName"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsCommon.Copy, "XrmSolutionComponents")]
    public class CopyXrmSolutionComponentsCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The unique name of the solution components to be moved from.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string FromSolutionName { get; set; }

        /// <summary>
        /// <para type="description">The unique name of the solution components to be moved to.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string ToSolutionName { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Moving Solution Components from {0} to {1}", FromSolutionName, ToSolutionName));

            using (var context = new CIContext(OrganizationService))
            {
                var fromSolutionId = GetSolutionId(context, FromSolutionName);
                var toSolutionId = GetSolutionId(context, ToSolutionName);

                var fromSolutionComponents = (from s in context.SolutionComponentSet
                            where s.SolutionId == new EntityReference(Solution.EntityLogicalName, fromSolutionId)
                            orderby s.RootSolutionComponentId descending
                            select new { s.Id, s.ComponentType, s.ObjectId, s.RootSolutionComponentId, s.IsMetadata }).ToList();
                var toSolutionComponents = (from s in context.SolutionComponentSet
                            where s.SolutionId == new EntityReference(Solution.EntityLogicalName, toSolutionId)
                            select new { s.Id, s.ComponentType, s.ObjectId, s.RootSolutionComponentId }).ToList();

                foreach (var solutionComponent in fromSolutionComponents)
                {                   
                    if (toSolutionComponents.Exists(depen => depen.ObjectId == solutionComponent.ObjectId && depen.ComponentType.Value == solutionComponent.ComponentType.Value))
                    {
                        continue;
                    }

                    var addReq = new AddSolutionComponentRequest()
                    {
                        ComponentId = (Guid)solutionComponent.ObjectId,
                        ComponentType = (int)solutionComponent.ComponentType.Value,
                        AddRequiredComponents = false,                        
                        SolutionUniqueName = ToSolutionName
                    };

                    if ((solutionComponent.IsMetadata ?? false) && fromSolutionComponents.Exists(depen => depen.RootSolutionComponentId == solutionComponent.Id))
                    {
                        addReq.DoNotIncludeSubcomponents = true;
                        base.WriteVerbose("DoNotIncludeSubcomponents set to true");
                    }

                    OrganizationService.Execute(addReq);
                    base.WriteVerbose(string.Format("Moved component from solution with Id : {0} and Type : {1}", solutionComponent.ObjectId, solutionComponent.ComponentType.Value));
                }
            }
        }

        private Guid GetSolutionId(CIContext context, string solutionName)
        {
            var query1 = from solution in context.SolutionSet
                         where solution.UniqueName == solutionName
                         select solution.Id;

            if (query1 == null)
            {
                throw new Exception(string.Format("Solution {0} could not be found", solutionName));
            }

            var solutionId = query1.FirstOrDefault();

            return solutionId;
        }

        #endregion
    } 
}