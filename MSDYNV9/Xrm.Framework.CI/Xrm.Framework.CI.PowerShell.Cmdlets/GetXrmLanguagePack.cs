using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Return available (provisioned) or installed language packs on CRM server</para>
    /// <para type="description">The Get-XrmLanguagePack cmdlet returns list of available (provisioned) or installed language packs on server.</para>
    /// <para type="description">Executes a "RetrieveProvisionedLanguagesRequest" or "RetrieveInstalledLanguagePacksRequest".</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Get-XrmLanguagePack -ConnectionString $connectionString -Type Installed</code>
    ///   <para>Returns list of language packs.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "XrmLanguagePack")]
    [OutputType(typeof(int[]))]
    public class GetXrmLanguagePack : XrmCommandBase
    {
        /// <summary>
        /// <para type="description">Type of result - Installed or Available/Provisioned</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public LanguagePackGetType Type { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (Type == LanguagePackGetType.Installed)
            {
                WriteVerbose("Installed language packs");
                WriteObject(((RetrieveInstalledLanguagePacksResponse)OrganizationService.Execute(new RetrieveInstalledLanguagePacksRequest())).RetrieveInstalledLanguagePacks);
            }
            else
            {
                WriteVerbose("Available (provisioned) language packs");
                WriteObject(((RetrieveProvisionedLanguagesResponse)OrganizationService.Execute(new RetrieveProvisionedLanguagesRequest())).RetrieveProvisionedLanguages);
            }
        }
    }

    public enum LanguagePackGetType
    {
        Installed,
        Available,
        Provisioned
    }
}