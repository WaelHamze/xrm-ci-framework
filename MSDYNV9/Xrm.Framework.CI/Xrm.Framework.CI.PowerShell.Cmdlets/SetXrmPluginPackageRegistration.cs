using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.Common.PluginRegistration;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Plugin Registration.</para>
    /// <para type="description">The Set-XrmPluginPackageRegistration cmdlet updates an existing Plugin Package and steps in CRM.
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Set-XrmPluginPackage -PackagePath $path -MappingJsonPath $jsonPath</code>
    ///   <para>Updates a Plugin Package and Steps.</para>
    /// </example>
    /// <para type="link" uri="http://msdn.microsoft.com/en-us/library/microsoft.xrm.sdk.messages.updaterequest.aspx">UpdateRequest.</para>
    [Cmdlet(VerbsCommon.Set, "XrmPluginPackageRegistration")]
    public class SetXrmPluginPackageRegistration : XrmCommandBase
    {
        #region Parameters

        [Parameter(Mandatory = true)]
        public RegistrationTypeEnum RegistrationType { get; set; }

        /// <summary>
        /// <para type="description">The full path to the package. e.g. C:\Solution\bin\release\Plugin.nupkg</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string PackagePath { get; set; }

        [Parameter(Mandatory = true)]
        public string SolutionName { get; set; }

        [Parameter(Mandatory = true)]
        public string PublisherPrefix { get; set; }

        [Parameter(Mandatory = false)]
        public string MappingFile { get; set; }

        #endregion Parameters

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose("Plugin Registration intitiated");

            var packageInfo = PackageInfo.GetPackageInfo(PackagePath);
            WriteVerbose($"Package Name: {packageInfo.PackageName}");

            using (var context = new CIContext(OrganizationService))
            {
                Assembly mappingPluginAssembly = null;
                var pluginRegistrationHelper = new PluginRegistrationHelper(OrganizationService, context, WriteVerbose, WriteWarning);
                WriteVerbose("PluginRegistrationHelper intiated");
                var pluginPackage = new Package
                {
                    Id = pluginRegistrationHelper.UpsertPluginPackage(packageInfo, SolutionName, PublisherPrefix, RegistrationType)
                };

                if (File.Exists(MappingFile))
                {
                    mappingPluginAssembly = pluginRegistrationHelper.ReadMappingFile(MappingFile);
                }
                else
                {
                    WriteVerbose($"UpsertPluingPackage {pluginPackage.Id} completed");
                    WriteVerbose("Plugin Package Registration completed");
                    return;
                }

                pluginRegistrationHelper.LoadPluginPackageAssemblies(pluginPackage);

                foreach (var pluginAssembly in pluginPackage.Assemblies.Where(x => x.Name == mappingPluginAssembly.Name))
                {
                    mappingPluginAssembly.Id = pluginAssembly.Id;
                    foreach (var type in mappingPluginAssembly.PluginTypes)
                    {
                        var pluginType = pluginAssembly.PluginTypes.FirstOrDefault(x => x.Name == type.Name);
                        if (pluginType != null)
                        {
                            type.Id = pluginType.Id;
                            pluginRegistrationHelper.UpsertPluginTypeAndSteps(mappingPluginAssembly.Id.Value, type, SolutionName, RegistrationType, true);
                        }
                    }
                }

                WriteVerbose($"UpsertPluginPackage {pluginPackage.Id} completed");
            }

            WriteVerbose("Plugin Package Registration completed");
        }

        #endregion Process Record
    }
}