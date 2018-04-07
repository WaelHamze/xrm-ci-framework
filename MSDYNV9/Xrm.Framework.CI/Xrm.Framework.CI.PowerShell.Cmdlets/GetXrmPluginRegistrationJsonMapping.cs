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
    [Cmdlet(VerbsCommon.Get, "XrmPluginRegistrationJsonMapping")]
    [OutputType(typeof(string))]
    public class GetXrmPluginRegistrationJsonMapping : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The name of assembly for which mapping file has to be created.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string AssemblyName { get; set; }

        [Parameter(Mandatory = true)]
        public bool IsWorkflowActivityAssembly { get; set; }

        [Parameter(Mandatory = true)]
        public String MappingJsonPath { get; set; }

        [Parameter(Mandatory = false)]
        public String SolutionName { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            string json = string.Empty;

            base.WriteVerbose("Get Plugin Registration Mapping intiated");

            using (var context = new CIContext(OrganizationService))
            {
                PluginRegistrationHelper pluginRegistrationHelper = new PluginRegistrationHelper(OrganizationService, context);
                base.WriteVerbose("PluginRegistrationHelper intiated");
                base.WriteVerbose(string.Format("Assembly Name: {0}", AssemblyName));
                base.WriteVerbose(string.Format("Mapping Json Path: {0}", MappingJsonPath));
                base.WriteVerbose(string.Format("Solution Name: {0}", SolutionName));
                var solutionId = pluginRegistrationHelper.GetSolutionId(SolutionName);
                base.WriteVerbose(string.Format("Solution Id: {0}", solutionId));
                if (IsWorkflowActivityAssembly)
                {
                    json = pluginRegistrationHelper.GetWorkflowActivityJsonMappingFromCrm(AssemblyName, solutionId);
                }
                else
                {
                    json = pluginRegistrationHelper.GetPluginJsonMappingFromCrm(AssemblyName, solutionId);
                }
                File.WriteAllText(MappingJsonPath, json);
            }

            base.WriteVerbose("Get Plugin Registration Mapping completed");
        }

        #endregion
    }
}
