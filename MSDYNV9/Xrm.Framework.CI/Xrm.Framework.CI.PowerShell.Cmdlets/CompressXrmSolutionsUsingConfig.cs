using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Xrm.Framework.CI.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Invokes a WhoAmIRequest</para>
    /// <para type="description">This cmdlet can be used to test your connectivity to CRM by calling 
    /// WhoAmIRequest and returning a WhoAmIResponse object.
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Export-XrmSolution -ConnectionString "" -EntityName "account"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsData.Compress, "XrmSolutions")]
    [OutputType(typeof(WhoAmIResponse))]
    public class CompressXrmSolutionsUsingConfig : CommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The absolute path to the solutionpackager.exe</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string SolutionPackagerPath { get; set; }

        /// <summary>
        /// <para type="description">The absolute path to the json config file containing pack configuration</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string ConfigFilePath { get; set; }

        /// <summary>
        /// <para type="description">The absolute path to the location of the packed solutions</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string OutputFolder { get; set; }

        /// <summary>
        /// <para type="description">The directory where the pack logs should be placed.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string LogsDirectory { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Logger.LogInformation("Packing Solution using config: {0}", ConfigFilePath);

            SolutionPackagerManager packagerManager = new SolutionPackagerManager(Logger);

            List<bool> results = packagerManager.PackSolutions(SolutionPackagerPath, OutputFolder, ConfigFilePath, LogsDirectory);

            if (results.Contains(false))
            {
                throw new System.Exception("Packing Solutions failed. Check logs for more information");
            }

            Logger.LogInformation("Packing Solutions Completed");
        }

        #endregion
    }
}