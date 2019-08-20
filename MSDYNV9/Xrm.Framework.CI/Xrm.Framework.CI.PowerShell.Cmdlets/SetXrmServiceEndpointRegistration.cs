using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Service Endpoint Registration.</para>
    /// <para type="description">The Set-XrmServiceEndpointRegistration cmdlet updates an existing Service Endpoint and steps in CRM.
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Set-XrmServiceEndpointRegistration -MappingJsonPath $jsonPath</code>
    ///   <para>Updates a Plugin Assembly and Steps.</para>
    /// </example>
    /// <para type="link" uri="http://msdn.microsoft.com/en-us/library/microsoft.xrm.sdk.messages.updaterequest.aspx">UpdateRequest.</para>
    [Cmdlet(VerbsCommon.Set, "XrmServiceEndpointRegistration")]
    public class SetXrmServiceEndpointRegistration : XrmCommandBase
    {
        #region Parameters

        [Parameter(Mandatory = true)]
        public RegistrationTypeEnum RegistrationType { get; set; }

        [Parameter(Mandatory = true)]
        public string MappingFile { get; set; }

        [Parameter(Mandatory = false)]
        public string SolutionName { get; set; }
        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (RegistrationType == RegistrationTypeEnum.Delsert)
            {
                WriteWarning("Registration Type not supported.");
                return;
            }

            WriteVerbose("Service Endpoint Registration intiated");
            
            using (var context = new CIContext(OrganizationService))
            {
                var pluginRegistrationHelper = new PluginRegistrationHelper(OrganizationService, context, WriteVerbose, WriteWarning);
                WriteVerbose("PluginRegistrationHelper intiated");

                if (File.Exists(MappingFile))
                {   
                    var serviceEndptLst = ReadServiceEndpointMappingFile(MappingFile);
                    pluginRegistrationHelper.UpsertServiceEndpoints(serviceEndptLst, SolutionName, RegistrationType);
                }
                else
                {
                    WriteError(new ErrorRecord(new ArgumentException("No operation performed. Mapping File not exists."), "FileMissing", ErrorCategory.InvalidArgument, MappingFile));
                }
            }

            WriteVerbose("Service Endpoint completed");
        }

        private List<ServiceEndpt> ReadServiceEndpointMappingFile(string MappingFile)
        {
            var fileInfo = new FileInfo(MappingFile);
            switch (fileInfo.Extension.ToLower())
            {
                case ".json":
                    WriteVerbose("Reading mapping json file");
                    var serviceEndptLst = Serializers.ParseJson<List<ServiceEndpt>>(MappingFile);
                    WriteVerbose("Deserialized mapping json file");
                    return serviceEndptLst;
                case ".xml":
                    WriteVerbose("Reading mapping xml file");
                    serviceEndptLst = Serializers.ParseXml<List<ServiceEndpt>>(MappingFile);
                    WriteVerbose("Deserialized mapping xml file");
                    return serviceEndptLst;
                default:
                    throw new ArgumentException("Only .json and .xml mapping files are supported", nameof(MappingFile));
            }
        }

        #endregion
    }
}