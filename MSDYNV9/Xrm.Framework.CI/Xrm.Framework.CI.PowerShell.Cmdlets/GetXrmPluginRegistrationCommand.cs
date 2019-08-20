using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Plugin Registration.</para>
    /// <para type="description">The Get-XrmPluginRegistration cmdlet reads an existing Plugin Assembly and steps in CRM.
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Get-XrmPluginRegistration -AssemblyPath $path -MappingFile $mappingFile</code>
    ///   <para>Reads a Plugin Assembly Types, Steps and Images.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "XrmPluginRegistration")]
    [OutputType(typeof(string))]
    public class GetXrmPluginRegistrationCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The name of assembly for which mapping file has to be created.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string AssemblyName { get; set; }

        /// <summary>
        /// <para type="description">The version of assembly for which mapping file has to be created.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string AssemblyVersion { get; set; }

        [Parameter(Mandatory = true)]
        public string MappingFile { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            WriteVerbose("Get Plugin Registration Mapping intiated");
            using (var context = new CIContext(OrganizationService))
            {
                PluginRegistrationHelper pluginRegistrationHelper = new PluginRegistrationHelper(OrganizationService, context, WriteVerbose, WriteWarning);
                WriteVerbose("PluginRegistrationHelper intiated");
                WriteVerbose($"Assembly Name: {AssemblyName}");
                WriteVerbose($"Assembly Version: {AssemblyVersion}");
                WriteVerbose($"Mapping Path: {MappingFile}");
                var assembly = pluginRegistrationHelper.GetAssemblyRegistration(AssemblyName, AssemblyVersion);
                pluginRegistrationHelper.SerializerObjectToFile(MappingFile, assembly);
            }

            WriteVerbose("Get Plugin Registration Mapping completed");
        }

        #endregion
    }
}
