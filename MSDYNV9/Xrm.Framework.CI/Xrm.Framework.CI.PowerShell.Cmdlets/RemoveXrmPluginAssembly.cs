using System;
using System.Linq;
using System.Management.Automation;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Remove a plugin assembly.</para>
    /// <para type="description">The Remove-XrmPluginAssembly cmdlet removes an existing Plugin Assembly in CRM.
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Remove-XrmPluginAssembly -AssemblyName $assemblyName</code>
    ///   <para>Plugin Assembly Name to Remove</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "XrmPluginAssembly")]
    public class RemoveXrmPluginAssembly : XrmCommandBase
    {
        #region Parameters
        /// <summary>
        /// <para type="description">The assembly name. e.g. Contoso.Plugin.dll</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string AssemblyName { get; set; }
        #endregion

        #region Process Record
        protected override void ProcessRecord()
        {
            using (var context = new CIContext(OrganizationService))
            {
                var pluginAssembly = context.PluginAssemblySet.SingleOrDefault(x => x.Name == AssemblyName);
                if (pluginAssembly == null)
                {
                    WriteWarning($"Couldn't find assembly in CRM: {AssemblyName}");
                    return;
                }

                var pluginRegistrationHelper = new PluginRegistrationHelper(OrganizationService, context, WriteVerbose, WriteWarning);
                pluginRegistrationHelper.DeleteObjectWithDependencies(pluginAssembly.Id, ComponentType.PluginAssembly);
                WriteVerbose($"Assembly {pluginAssembly.Name} / {pluginAssembly.Id} removed from CRM");
            }
        }
        #endregion
    }
}