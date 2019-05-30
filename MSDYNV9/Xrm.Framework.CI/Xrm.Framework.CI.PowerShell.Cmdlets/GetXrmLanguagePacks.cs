using System.Management.Automation;

using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Return available (provisioned) or installed language packs on CRM server</para>
    /// <para type="description">The Get-XrmLanguagePacks cmdlet returns list of available (provisioned) or installed language packs on server.</para>
    /// <para type="description">Executes a "RetrieveProvisionedLanguagesRequest" or "RetrieveInstalledLanguagePacksRequest".</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Get-XrmLanguagePacks -ConnectionString $connectionString -Type Installed</code>
    ///   <para>Returns list of language packs.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "XrmLanguagePacks")]
    [OutputType(typeof(int[]))]
    public class GetXrmLanguagePacks : XrmCommandBase
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
                WriteObject(OrganizationService.GetInstalledLanguages());
            }
            else
            {
                WriteVerbose("Available (provisioned) language packs");
                WriteObject(OrganizationService.GetProvisionedLanguages());
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