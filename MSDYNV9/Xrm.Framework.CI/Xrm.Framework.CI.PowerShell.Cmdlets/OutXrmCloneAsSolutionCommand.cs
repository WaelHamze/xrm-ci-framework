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
    /// <para type="synopsis">Colne a CRM Solution.</para>
    /// <para type="description">The Out-XrmCloneAsSolution clone solution from solution.
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Out-XrmCloneAsSolution -ConnectionString "" -DisplayName "Solution display name" -ParentSolutionUniqueName "UniqueSolutionName" -VersionNumber "1.0.0.1"</code>
    /// </example>
    [Cmdlet(VerbsData.Out, "XrmCloneAsSolution")]
    public class OutXrmCloneAsSolutionCommand : XrmCommandBase
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
        [Parameter(Mandatory = true)]
        public string VersionNumber { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose("Executing CloneAsSolutionRequest");
            var cloneAsPatch = new CloneAsSolutionRequest
            {
                DisplayName = DisplayName,
                ParentSolutionUniqueName = ParentSolutionUniqueName,
                VersionNumber = VersionNumber,
            };

            OrganizationService.Execute(cloneAsPatch);
        }

        #endregion
    } 
}