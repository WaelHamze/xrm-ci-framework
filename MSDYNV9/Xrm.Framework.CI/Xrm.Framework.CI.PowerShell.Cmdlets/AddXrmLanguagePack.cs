using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Provision a new language on CRM server</para>
    /// <para type="description">The Add-XrmLanguagePack cmdlet provision a new language on CRM server. Language pack must be installed before on the server.</para>
    /// <para type="description">It's recommended to set a high timeout (Timeout parameter).</para>
    /// <para type="description">Executes a "ProvisionLanguageRequest".</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Add-XrmLanguagePack -Language 1045 -ConnectionString $connectionString -Timeout 900</code>
    ///   <para>Provision a new language on CRM server.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "XrmLanguagePack")]
    public class AddXrmLanguagePack : XrmCommandBase
    {
        /// <summary>
        /// <para type="description">Sets the language to provision. Required.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public int Language { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var availableLanguagesPacks = ((RetrieveProvisionedLanguagesResponse)OrganizationService.Execute(new RetrieveProvisionedLanguagesRequest())).RetrieveProvisionedLanguages;
            var installedLanguagePacks = ((RetrieveInstalledLanguagePacksResponse)OrganizationService.Execute(new RetrieveInstalledLanguagePacksRequest())).RetrieveInstalledLanguagePacks;

            if (availableLanguagesPacks.Contains(Language))
            {
                WriteVerbose($"LanguagePack {Language} already provisioned");
                return;
            }

            if (!installedLanguagePacks.Contains(Language))
            {
                throw new ArgumentException($"LanguagePack {Language} is not installed on server", nameof(Language));
            }

            if (Timeout == 0)
            {
                WriteWarning($"It's recommended to use Timeout parameter for this cmdlet. Language provisioning will continue on server if this cmdlet timeout.");
            }

            OrganizationService.Execute(new ProvisionLanguageRequest
            {
                Language = Language
            });
            WriteVerbose($"LanguagePack {Language} sucessfully provisioned.");
        }
    }
}