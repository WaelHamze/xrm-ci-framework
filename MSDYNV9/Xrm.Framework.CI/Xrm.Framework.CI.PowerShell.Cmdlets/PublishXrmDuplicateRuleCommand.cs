using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Publishes a CRM duplicate rule</para>
    /// <para type="description">This cmdlet publishes a specific CRM duplicate rule
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Publish-XrmDuplicateRule -ConnectionString "" -DuplicateRuleId $duplicateRuleId</code>
    ///   <para>Publishes a existing CRM duplicate rule with the Id $duplicateRuleId</para>
    /// </example>
    [Cmdlet(VerbsData.Publish, "XrmDuplicateRule")]
    public class PublishXrmDuplicateRuleCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// Id of a CRM duplicate rule to be published
        /// </summary>
        [Parameter(Mandatory = true)]
        public Guid DuplicateRuleId { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Publishing a CRM duplicate rule with Id: {0}", DuplicateRuleId));

            var publishAllXmlRequest = new PublishDuplicateRuleRequest
            {
                DuplicateRuleId = DuplicateRuleId
            };
            OrganizationService.Execute(publishAllXmlRequest);

            base.WriteVerbose("Publish duplicate rule request sent out successfully");
        }

        #endregion
    }
}
