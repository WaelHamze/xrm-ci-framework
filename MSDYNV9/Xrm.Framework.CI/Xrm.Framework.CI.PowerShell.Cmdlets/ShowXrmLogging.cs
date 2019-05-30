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
    [Cmdlet(VerbsCommon.Show, "XrmLogging")]
    public class ShowXrmLogging : XrmCommandBase
    {
        #region Parameters

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Logger.LogVerbose("This is a verbose message.");
            Logger.LogInformation("This is an information message.");
            Logger.LogWarning("This is a warning message.");
            Logger.LogError("This is an error message.");
        }

        #endregion
    } 
}