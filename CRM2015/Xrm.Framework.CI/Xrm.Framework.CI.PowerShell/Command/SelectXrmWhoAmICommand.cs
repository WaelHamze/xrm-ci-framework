using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;

namespace Xrm.Framework.CI.PowerShell.Command
{
    /// <summary>
    /// <para type="synopsis">Invokes a WhoAmIRequest</para>
    /// <para type="description">This cmdlet can be used to test your connectivity to CRM by calling 
    /// WhoAmIRequest and returning a WhoAmIResponse object.
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Export-XrmSolution -ConnectionString "" -EntityName "account"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsCommon.Select, "WhoAmI")]
    [OutputType(typeof(WhoAmIResponse))]
    public class SelectXrmWhoAmICommand : XrmCommandBase
    {
        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Invoking Organization Service"));

            var response = (OrganizationService.Execute(new WhoAmIRequest()) as WhoAmIResponse);

            base.WriteObject(response);

            base.WriteVerbose(string.Format("OrganizationId: {0}", response.OrganizationId));
            base.WriteVerbose(string.Format("BusinessUnitId: {0}", response.BusinessUnitId));
            base.WriteVerbose(string.Format("UserId: {0}", response.UserId));
        }

        #endregion
    }
}