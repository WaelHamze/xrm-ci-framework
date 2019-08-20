   using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Service Endpoint Registration.</para>
    /// <para type="description">The Get-XrmServiceEndpointRegistration cmdlet reads an existing Service Endpoints and steps in CRM.
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Get-XrmServiceEndpointRegistration -MappingFile $mappingFile</code>
    ///   <para>Reads a Service Endpoint and Steps.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "XrmServiceEndpointRegistration")]
    [OutputType(typeof(string))]
    public class GetXrmServiceEndpointRegistrationCommand : XrmCommandBase
    {
        #region Parameters

        [Parameter(Mandatory = false)]
        public string EndpointName { get; set; }

        [Parameter(Mandatory = true)]
        public string MappingFile { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            WriteVerbose("Get Service Endpoint Registration Mapping intiated");
            using (var context = new CIContext(OrganizationService))
            {
                PluginRegistrationHelper pluginRegistrationHelper = new PluginRegistrationHelper(OrganizationService, context, WriteVerbose, WriteWarning);
                WriteVerbose("PluginRegistrationHelper intiated");
                WriteVerbose($"Mapping Path: {MappingFile}");
                WriteVerbose($"Endpoint Name: {EndpointName}");
                var webHookList = pluginRegistrationHelper.GetServiceEndpoints(string.Empty, EndpointName);
                pluginRegistrationHelper.SerializerObjectToFile(MappingFile, webHookList);
            }

            WriteVerbose("Get Plugin Registration Mapping completed");
        }

        #endregion
    }
}
