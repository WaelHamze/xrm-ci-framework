using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Publishes all customizations</para>
    /// <para type="description">This cmdlet publishes all CRM cusotmisations
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Export-XrmSolution -ConnectionString "" -EntityName "account"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsData.Publish, "XrmCustomizations")]
    public class PublishXrmCustomizationsCommand : XrmCommandBase
    {
        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Publishing Customizations"));

            var publishAllXmlRequest = new PublishAllXmlRequest();
            OrganizationService.Execute(publishAllXmlRequest);

            base.WriteVerbose(string.Format("Publish Customizations Completed"));
        }

        #endregion
    }
}