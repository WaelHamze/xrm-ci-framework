using System;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Threading;

using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

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

        [Parameter(Mandatory = false)]
        public bool Async { get; set; } = true;

        /// <summary>
        /// <para type="description">Specify the timeout duration for waiting on async mode to complete. Default = 60 minutes</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int AsyncWaitTimeout { get; set; } = 60 * 60;

        /// <summary>
        /// <para type="description">The sleep interval between checks on the provisioning progress. Default = 15 seconds</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int SleepInterval { get; set; } = 15;

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var availableLanguagesPacks = OrganizationService.GetProvisionedLanguages();
            var installedLanguagePacks = OrganizationService.GetInstalledLanguages();

            if (availableLanguagesPacks.Contains(Language))
            {
                WriteVerbose($"LanguagePack {Language} already provisioned");
                return;
            }

            if (!installedLanguagePacks.Contains(Language))
            {
                throw new ArgumentException($"LanguagePack {Language} is not installed on server", nameof(Language));
            }

            if (!Async && Timeout == 0)
            {
                WriteWarning($"It's recommended to use Timeout parameter for this cmdlet. Language provisioning will continue on server if this cmdlet timeout.");
            }

            WriteVerbose("Executing ProvisionLanguageRequest");
            var stopwatch = Stopwatch.StartNew();
            if (Async)
            {
                var end = DateTime.Now.AddSeconds(AsyncWaitTimeout);
                try
                {
                    OrganizationService.ProvisionLanguage(Language);
                }
                catch (TimeoutException ex)
                {
                    WriteVerbose($"Synchronous operation aborted after {stopwatch.Elapsed} with message {ex.Message}. But please be patient - operation will go forward in background.");
                }

                while (!OrganizationService.GetProvisionedLanguages().Contains(Language))
                {
                    if (end < DateTime.Now)
                    {
                        throw new Exception($"Asynchronous wait timeout after {stopwatch.Elapsed}");
                    }

                    Thread.Sleep(SleepInterval * 1000);
                    WriteVerbose($"Sleeping for {SleepInterval} seconds");
                }
            }
            else
            {
                OrganizationService.ProvisionLanguage(Language);
            }
            WriteVerbose($"LanguagePack {Language} sucessfully provisioned in {stopwatch.Elapsed}.");
        }
    }
}