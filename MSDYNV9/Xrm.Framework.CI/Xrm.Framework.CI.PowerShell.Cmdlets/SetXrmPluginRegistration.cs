using Newtonsoft.Json;
using System;
using System.IO;
using System.Management.Automation;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Plugin Registration.</para>
    /// <para type="description">The Set-XrmPluginRegistration cmdlet updates an existing Plugin Assembly and steps in CRM.
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Set-XrmPluginRegistration -AssemblyPath $path -MappingJsonPath $jsonPath</code>
    ///   <para>Updates a Plugin Assembly and Steps.</para>
    /// </example>
    /// <para type="link" uri="http://msdn.microsoft.com/en-us/library/microsoft.xrm.sdk.messages.updaterequest.aspx">UpdateRequest.</para>
    [Cmdlet(VerbsCommon.Set, "XrmPluginRegistration")]
    public class SetXrmPluginRegistration : XrmCommandBase
    {
        #region Parameters

        [Parameter(Mandatory = true)]
        public RegistrationTypeEnum RegistrationType { get; set; }

        /// <summary>
        /// <para type="description">The full path to the assembly. e.g. C:\Solution\bin\release\Plugin.dll</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string AssemblyPath { get; set; }

        [Parameter(Mandatory = false)]
        public bool UseSplitAssembly { get; set; }

        [Parameter(Mandatory = false)]
        public string ProjectFilePath { get; set; }

        [Parameter(Mandatory = false)]
        public string MappingFile { get; set; }

        [Parameter(Mandatory = false)]
        public string SolutionName { get; set; }
        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose("Plugin Registration intiated");

            if (UseSplitAssembly)
            {
                if (!File.Exists(ProjectFilePath)) throw new Exception("Project File Path is required if you want to split assembly.");
                if (RegistrationType == RegistrationTypeEnum.Delsert) throw new Exception("Registration type 'Remove Plugin Types and Steps which are not in mapping and Upsert' will not work when 'Split Assembly' is enabled.");
                if (!File.Exists(MappingFile)) throw new Exception("Mapping Json Path is required if you want to split assembly.");
            }

            var assemblyInfo = AssemblyInfo.GetAssemblyInfo(AssemblyPath);
            WriteVerbose($"Assembly Name: {assemblyInfo.AssemblyName}");
            WriteVerbose($"Assembly Version: {assemblyInfo.Version}");

            using (var context = new CIContext(OrganizationService))
            {
                var pluginRegistrationHelper = new PluginRegistrationHelper(OrganizationService, context, WriteVerbose, WriteWarning);
                WriteVerbose("PluginRegistrationHelper intiated");
                Assembly pluginAssembly = null;
                Guid pluginAssemblyId = Guid.Empty;

                if (File.Exists(MappingFile))
                {
                    pluginAssembly = pluginRegistrationHelper.ReadMappingFile(MappingFile);
                    pluginAssemblyId = pluginAssembly.Id ?? Guid.Empty;
                }
                else
                {
                    pluginAssemblyId = pluginRegistrationHelper.UpsertPluginAssembly(pluginAssembly, assemblyInfo, SolutionName, RegistrationType);
                    WriteVerbose($"UpsertPluginAssembly {pluginAssemblyId} completed");
                    WriteVerbose("Plugin Registration completed");
                    return;
                }

                if (pluginAssembly == null)
                {
                    WriteVerbose("Plugin Registration completed");
                    return;
                }

                if (pluginAssembly.PluginTypes == null)
                {
                    WriteVerbose("No mapping found for types.");
                    WriteVerbose("Plugin Registration completed");
                    return;
                }

                if (RegistrationType == RegistrationTypeEnum.Delsert)
                {
                    WriteVerbose($"RemoveComponentsNotInMapping {assemblyInfo.AssemblyName} started");
                    pluginRegistrationHelper.RemoveComponentsNotInMapping(pluginAssembly);
                    WriteVerbose($"RemoveComponentsNotInMapping {assemblyInfo.AssemblyName} completed");
                    RegistrationType = RegistrationTypeEnum.Upsert;
                }

                if (UseSplitAssembly)
                {
                    foreach (var type in pluginAssembly.PluginTypes)
                    {
                        UploadSplitAssembly(assemblyInfo, pluginRegistrationHelper, type);
                    }
                }
                else
                {
                    WriteVerbose($"UpsertPluginAssembly {pluginAssemblyId} started");
                    pluginAssemblyId = pluginRegistrationHelper.UpsertPluginAssembly(pluginAssembly, assemblyInfo, SolutionName, RegistrationType);
                    WriteVerbose($"UpsertPluginAssembly {pluginAssemblyId} completed");

                    foreach (var type in pluginAssembly.PluginTypes)
                    {
                        pluginRegistrationHelper.UpsertPluginTypeAndSteps(pluginAssemblyId, type, SolutionName, RegistrationType);
                    }
                }
            }
            WriteVerbose("Plugin Registration completed");
        }

       private void UploadSplitAssembly(AssemblyInfo assemblyInfo, PluginRegistrationHelper pluginRegistrationHelper, Xrm.Framework.CI.Common.Type type)
        {
            var temp = new FileInfo(ProjectFilePath);
            var splitAssembly = AssemblyInfo.GetAssemblyInfo(assemblyInfo.AssemblyDirectory.Replace(temp.DirectoryName, temp.DirectoryName + type.Name) + "\\" + type.Name + ".dll");
            var pluginAssemblyId = pluginRegistrationHelper.UpsertPluginAssembly(null, splitAssembly, SolutionName, RegistrationType);
            WriteVerbose($"UpsertPluginAssembly {pluginAssemblyId} completed");

            pluginRegistrationHelper.UpsertPluginTypeAndSteps(pluginAssemblyId, type, SolutionName, RegistrationType);
        }

        #endregion
    }
}