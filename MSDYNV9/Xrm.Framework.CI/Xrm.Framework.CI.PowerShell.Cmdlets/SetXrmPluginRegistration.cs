using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

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

        /// <summary>
        /// <para type="description">The full path to the assembly. e.g. C:\Solution\bin\release\Plugin.dll</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public String AssemblyPath { get; set; }

        [Parameter(Mandatory = true)]
        public String MappingJsonPath { get; set; }

        [Parameter(Mandatory = false)]
        public String SolutionName { get; set; }
        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose("Plugin Registration intiated");

            FileInfo assemblyInfo = new FileInfo(AssemblyPath);
            var lastIndex = assemblyInfo.Name.LastIndexOf(".dll");
            string assemblyName = lastIndex > 0 ? assemblyInfo.Name.Remove(lastIndex, 4) : assemblyInfo.Name;
            string version = FileVersionInfo.GetVersionInfo(AssemblyPath).FileVersion;
            string content = Convert.ToBase64String(File.ReadAllBytes(AssemblyPath));
           
            base.WriteVerbose(string.Format("Assembly Name: {0}", assemblyName));
            base.WriteVerbose(string.Format("Assembly Version: {0}", version));

            using (var context = new CIContext(OrganizationService))
            {                
                PluginRegistrationHelper pluginRegistrationHelper = new PluginRegistrationHelper(OrganizationService, context);
                base.WriteVerbose("PluginRegistrationHelper intiated");
                string json = File.ReadAllText(MappingJsonPath);
                var pluginAssembly = JsonConvert.DeserializeObject<Assembly>(json);
                
                var pluginAssemblyId = pluginRegistrationHelper.UpsertPluginAssembly(pluginAssembly, version, content, SolutionName);
                base.WriteVerbose(string.Format("UpsertPluginAssembly {0} completed", pluginAssemblyId));
                foreach (var type in pluginAssembly.PluginTypes)
                {
                    var pluginTypeId = pluginRegistrationHelper.UpsertPluginType(pluginAssemblyId, type, SolutionName);
                    base.WriteVerbose(string.Format("UpsertPluginType {0} completed", pluginTypeId));
                    foreach (var step in type.Steps)
                    {
                        var sdkMessageProcessingStepId = pluginRegistrationHelper.UpsertSdkMessageProcessingStep(pluginTypeId, step, SolutionName);
                        base.WriteVerbose(string.Format("UpsertSdkMessageProcessingStep {0} completed", sdkMessageProcessingStepId));
                        foreach (var image in step.Images)
                        {
                            var sdkMessageProcessingStepImageId = pluginRegistrationHelper.UpsertSdkMessageProcessingStepImage(sdkMessageProcessingStepId, image, SolutionName);
                            base.WriteVerbose(string.Format("UpsertSdkMessageProcessingStepImage {0} completed", sdkMessageProcessingStepImageId));
                        }
                    }
                }
            }

            base.WriteVerbose("Plugin Registration completed");
        }

        #endregion
    }
}
