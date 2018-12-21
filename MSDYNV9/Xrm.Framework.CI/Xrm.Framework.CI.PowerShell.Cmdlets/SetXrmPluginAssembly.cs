using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Updates a plugin assembly.</para>
    /// <para type="description">The Set-PluginAssembly cmdlet updates an existing Plugin Assembly in CRM.
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Set-PluginAssembly -Path $path</code>
    ///   <para>Updates a Plugin Assembly.</para>
    /// </example>
    /// <para type="link" uri="http://msdn.microsoft.com/en-us/library/microsoft.xrm.sdk.messages.updaterequest.aspx">UpdateRequest.</para>
    [Cmdlet(VerbsCommon.Set, "XrmPluginAssembly")]
    public class SetXrmPluginAssembly : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The full path to the assembly. e.g. C:\Solution\bin\release\Plugin.dll</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public String Path { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            var assembly = System.Reflection.Assembly.LoadFile(Path);
            var assemblyName = assembly.GetName().Name;
            var version = assembly.GetName().Version.ToString();

            WriteObject($"Reading Assembly: {Path}");
            WriteObject($"Assembly Name: {assemblyName}");
            WriteObject($"Assembly Version: {version}");

            using (var context = new CIContext(OrganizationService))
            {
                var pluginAssembly = context.PluginAssemblySet.Single(x => x.Name == assemblyName);
                var content = Convert.ToBase64String(File.ReadAllBytes(Path));
                if (pluginAssembly.Version != version ||
                    pluginAssembly.Content.GetHashCode() != content.GetHashCode())
                {
                    WriteObject($"Updating Plugin Assembly: {assemblyName}");
                    pluginAssembly.Content = content;
                    context.UpdateObject(pluginAssembly);
                    context.SaveChanges();
                }
                else
                    WriteObject($"Assembly version and content were not changed. Skipping update.");
            }

        }

        #endregion
    }
}