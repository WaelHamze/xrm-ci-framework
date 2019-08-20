using System;
using System.IO;
using System.Management.Automation;
using Xrm.Framework.CI.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Generate JSON PluginRegistration from Custom Attribute Class</para>
    /// <para type="description">This cmdlet generates JSON used to register plugin and workflow assemblies
    ///   using the Set-XrmPluginRegistration command. It uses a custom attribute class, teh code for which can be 
    ///   obtained using the Get-XrmPluginRegistrationClass
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>Get-XrmPluginRegistrationFromAssembly -AssemblyPath "C:\repos\DemonstratePlugin.dll" -MappingFile "C:\repos\PluginRegistration.json"</code>
    ///   <para>Outputs plugin registration JSON to C:\repos\PluginRegistration.json, built from classes using
    ///     custom attribute in C:\repos\DemonstratePlugin.dll. USes default custom attribute class name
    ///   </para>
    /// </example>
    ///    /// <example>
    ///   <code>Get-XrmPluginRegistrationFromAssembly -AssemblyPath "C:\repos\DemonstratePlugin.dll" -MappingFile "C:\repos\PluginRegistration.json" -CustomAttributeClassName "Common.MyClass"</code>
    ///   <para>Outputs plugin registration JSON to C:\repos\PluginRegistration.json, built from classes using
    ///     custom attribute in C:\repos\DemonstratePlugin.dll. Searches for Custom Attribute class named Common.MyClass
    ///   </para>
    /// </example>
    [Cmdlet("Get", "XrmPluginRegistrationFromAssembly")]
    [OutputType(typeof(string))]
    public class GetXrmPluginRegistrationFromAssemblyCommand : Cmdlet
    {
        [Parameter(Mandatory = true)] public string AssemblyPath { get; set; }
        [Parameter(Mandatory = true)] public string MappingFile { get; set; }
        [Parameter(Mandatory = false)] public bool? MaintainIdsAndSteps { get; set; }
        [Parameter(Mandatory = false)] public string CustomAttributeClassName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            WriteVerbose("Get Plugin Registration from Assembly initiated");

            if (string.IsNullOrEmpty(MappingFile) || !File.Exists(MappingFile))
                throw new Exception($"{nameof(MappingFile)} must exist");

            if (string.IsNullOrEmpty(CustomAttributeClassName))
                CustomAttributeClassName = "XrmCiFramework.XrmCiPluginRegistration";

            var pluginRegistrationHelper = new PluginRegistrationHelper(WriteVerbose, WriteWarning);
            
            var assembly =
                pluginRegistrationHelper.GetPluginRegistrationObject(AssemblyPath, CustomAttributeClassName) as Assembly;

            if (!MaintainIdsAndSteps.HasValue || (MaintainIdsAndSteps.HasValue && MaintainIdsAndSteps == true))
            {
                var assemblyFromMappingFile = pluginRegistrationHelper.ReadMappingFile(MappingFile);
                if(assemblyFromMappingFile != null)
                    assembly += assemblyFromMappingFile;
            }

            pluginRegistrationHelper.SerializerObjectToFile(MappingFile, assembly);
            WriteVerbose("Get Plugin Registration from Assembly completed");
        }
    }
}
