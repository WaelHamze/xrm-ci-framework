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
    /// <para type="synopsis">Removes a CRM solution</para>
    /// <para type="description">This cmdlet deletes a CRM solution 
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Remove-XrmSolution -ConnectionString "" -SolutionName "SolutionName"</code>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "XrmSolution")]
    public class RemoveXrmSolutionCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The absolute path to the solution file to be imported</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string SolutionName { get; set; }

        
        public RemoveXrmSolutionCommand()
        {
        }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Removing Solution: {0}", SolutionName));


            QueryExpression queryImportedSolution = new QueryExpression
            {
                EntityName = Solution.EntityLogicalName,
                ColumnSet = new ColumnSet(new string[] { "solutionid", "friendlyname" }),
                Criteria = new FilterExpression()
            };


            queryImportedSolution.Criteria.AddCondition("uniquename", ConditionOperator.Equal, SolutionName);

            Solution ImportedSolution = (Solution)OrganizationService.RetrieveMultiple(queryImportedSolution).Entities[0];

            OrganizationService.Delete(Solution.EntityLogicalName, (Guid)ImportedSolution.SolutionId);

            base.WriteVerbose(string.Format("Solution {0} Deleted", SolutionName));
        }

       
        #endregion
    }
}