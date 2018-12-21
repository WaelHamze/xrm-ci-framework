using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

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
    /// <example>
    ///   <code>C:\PS>Publish-XrmDuplicateRule -ConnectionString "" -DuplicateRuleName $duplicateRuleName</code>
    ///   <para>Publishes a existing CRM duplicate rule with the name $duplicateRuleName</para>
    /// </example>
    [Cmdlet(VerbsData.Publish, "XrmDuplicateRule")]
    public class PublishXrmDuplicateRuleCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// Id of a CRM duplicate rule to be published
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "PublishByDuplicateRuleId")]
        public Guid DuplicateRuleId { get; set; }

        /// <summary>
        /// Name of a CRM duplicate rule to be published
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "PublishByDuplicateRuleName")]
        public string DuplicateRuleName { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Guid duplicateRuleId = DuplicateRuleId != Guid.Empty ? DuplicateRuleId : GetDuplicateRuleId(DuplicateRuleName);

            base.WriteVerbose(string.Format("Publishing a CRM duplicate rule with Id: {0}", duplicateRuleId));

            var publishAllXmlRequest = new PublishDuplicateRuleRequest
            {
                DuplicateRuleId = duplicateRuleId
            };
            OrganizationService.Execute(publishAllXmlRequest);

            base.WriteVerbose("Publish duplicate rule request sent out successfully");
        }

        private Guid GetDuplicateRuleId(string duplicateRuleName)
        {
            using (var context = new CIContext(OrganizationService))
            {
                base.WriteVerbose(string.Format("Querying a duplicate rule with the name: {0}", duplicateRuleName));

                var query = from rule in context.DuplicateRuleSet
                    where rule.Name == duplicateRuleName
                    select new DuplicateRule
                    {
                        Id = rule.Id,
                        Name = rule.Name
                    };

                DuplicateRule duplicateRule = query.FirstOrDefault();

                if (duplicateRule == null)
                {
                    var error = string.Format("Duplicate rule with the name '{0}' could not be found", duplicateRuleName);
                    throw new ArgumentOutOfRangeException(nameof(duplicateRuleName), error);
                }

                base.WriteVerbose(string.Format("Found a duplicate rule with the Id: {0}", duplicateRule.Id));

                return duplicateRule.Id;
            }
        }

        #endregion
    }
}
