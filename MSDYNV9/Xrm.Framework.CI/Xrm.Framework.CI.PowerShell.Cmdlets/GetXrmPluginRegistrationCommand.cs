using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;
using Xrm.Framework.CI.PowerShell.Cmdlets.PluginRegistration;

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
                WriteVerbose($"Mapping Path: {MappingFile}");
                var fileInfo = new FileInfo(MappingFile);
                switch (fileInfo.Extension.ToLower())
                {
                    case ".json":
                        var assembly = pluginRegistrationHelper.GetAssemblyRegistration(AssemblyName);
                        Serializers.SaveJson(MappingFile, assembly);
                        break;
                    case ".xml":
                        assembly = pluginRegistrationHelper.GetAssemblyRegistration(AssemblyName);
                        Serializers.SaveXml(MappingFile, assembly);
                        break;
                    default:
                        throw new ArgumentException("Only .json and .xml mapping files are supported", nameof(MappingFile));
                }
            }

            WriteVerbose("Get Plugin Registration Mapping completed");
        }

        #endregion
    }
}
