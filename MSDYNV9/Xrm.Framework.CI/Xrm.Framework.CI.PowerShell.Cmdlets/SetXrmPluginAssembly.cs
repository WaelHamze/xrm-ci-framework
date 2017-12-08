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
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Updating Assembly: {0}", Path));

            FileInfo assemblyInfo = new FileInfo(Path);

            String assemblyName = assemblyInfo.Name.Replace(".dll", "");
            String version = FileVersionInfo.GetVersionInfo(Path).FileVersion;
            String content = Convert.ToBase64String(File.ReadAllBytes(Path));

            base.WriteVerbose(string.Format("Assembly Name: {0}", assemblyName));
            base.WriteVerbose(string.Format("Assembly Version: {0}", version));

            using (var context = new CIContext(OrganizationService))
            {
                var query = from a in context.PluginAssemblySet
                            where a.Name == assemblyName
                            select a.Id;

                Guid Id = query.FirstOrDefault();

                if (Id == null || Id == Guid.Empty)
                {
                    throw new ItemNotFoundException(string.Format("{0} was not found", assemblyName));
                }

                PluginAssembly pluginAssembly = new PluginAssembly()
                {
                    Id = Id,
                    Version = version,
                    Content = content
                };

                OrganizationService.Update(pluginAssembly);
            }

            base.WriteVerbose(string.Format("Assembly Updated: {0}", Path));
        }

        #endregion
    }
}
