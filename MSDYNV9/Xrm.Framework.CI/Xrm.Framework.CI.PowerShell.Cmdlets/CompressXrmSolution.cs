using System;
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
    [Cmdlet(VerbsData.Compress, "XrmSolution")]
    [OutputType(typeof(WhoAmIResponse))]
    public class CompressXrmSolution : CommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The absolute path to the solutionpackager.exe</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string SolutionPackagerPath { get; set; }

        /// <para type="description">The package type used by the solution packager</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string PackageType { get; set; }


        /// <para type="description">The folder containing the unpacked customizations</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Folder { get; set; }

        /// <summary>
        /// <para type="description">The mapping file used by the solution packager</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string MappingFile { get; set; }

        /// <summary>
        /// <para type="description">The new version of the solution to set in solution.xml</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string Version { get; set; }

        /// <summary>
        /// <para type="description">Set to true to include the version number in the generated solution file</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool IncludeVersionInName { get; set; }


        /// <summary>
        /// <para type="description">Set to true to update solution release version</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool IncrementReleaseVersion { get; set; }

        /// <summary>
        /// <para type="description">Set to true to cause operation to fail if warnings are encountered</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool TreatWarningsAsErrors { get; set; }

        /// <summary>
        /// <para type="description">The absolute path to the location of the packed solutions</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string OutputFolder { get; set; }

        /// <summary>
        /// <para type="description">This argument generates a template resource file</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string SourceLoc { get; set; }

        /// <summary>
        /// <para type="description">Extract or merge all string resources into .resx files</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool Localize { get; set; }

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

            Logger.LogInformation("Packing Solution from Path: {0}", Folder);

            SolutionPackagerManager packagerManager = new SolutionPackagerManager(Logger);

            SolutionPackager_PackageType packageType;
            if (!Enum.TryParse<SolutionPackager_PackageType>(PackageType, out packageType))
            {
                throw new Exception($"{PackageType} is not valid");
            }

            bool result = packagerManager.PackSolution(
                SolutionPackagerPath,
                OutputFolder,
                Folder,
                packageType,
                IncludeVersionInName,
                MappingFile,
                SourceLoc,
                Localize,
                TreatWarningsAsErrors,
                IncrementReleaseVersion,
                Version,
                LogsDirectory);

            if (!result)
            {
                throw new System.Exception("Packing Solution failed. Check logs for more information");
            }

            Logger.LogInformation("Packing Solution Completed");
        }

        #endregion
    }
}