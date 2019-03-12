using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;

using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Updates security roles for the specified dashboard in CRM.</para>
    /// <para type="description">The Set-XrmDashboardRole cmdlet updates roles for specified dashboard in CRM. Useful for OOB dashbards.</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Set-XrmDashboardRole -Name $name -RoleNames "System Administrator"</code>
    ///   <code>C:\PS>Set-XrmDashboardRole -Id $id -RoleNames "Sales Manager,Salesperson"</code>
    ///   <para>Updates security roles for dashbard specified by Name or Id.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "XrmDashboardRole")]
    public class SetXrmDashboardRoleCommand : XrmCommandBase
    {
        #region Parameters

        private const string findByName = "FindByName";
        private const string findById = "FindById";
        private const int Dashboard = 0;

        /// <summary>
        /// <para type="description">Name of Dashboard to update, eq. "Microsoft Dynamics CRM Overview"</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = findByName)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">Name of Dashboard to update, eq. "00000000-0000-0000-0000-000000000000"</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = findById)]
        public Guid Id { get; set; }

        /// <summary>
        /// <para type="description">Role names to set, eq. "Sales Manager,Salesperson". Can be empty to allow all roles access dashboard.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public String RoleNames { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose($"ParameterSetName: {ParameterSetName}");
            WriteVerbose($"Id: {Id}");
            WriteVerbose($"Name: {Name}");
            WriteVerbose($"RoleNames: {RoleNames}");

            string filteringKey;
            string filteringValue;
            Func<SystemForm, bool> predicateFunction;

            if (ParameterSetName == findByName)
            {
                filteringKey = "name";
                filteringValue = Name;
                predicateFunction = x => x.Name == Name;
            }
            else
            {
                filteringKey = "id";
                filteringValue = Id.ToString();
                predicateFunction = x => x.Id == Id;
            }

            using (var ctx = new CIContext(OrganizationService))
            {
                List<SystemForm> dashboards;
                dashboards = ctx.SystemFormSet.Where(predicateFunction).ToList();

                if (dashboards.Count == 0)
                {
                    WriteWarning($"Couldn't find dashboard with {filteringKey} {filteringValue}");
                    return;
                }

                if (dashboards.Count > 1)
                {
                    WriteWarning($"There are more than one dashboard with name {Name}. Try execute cmdlet using Id.");
                    return;
                }

                var dashboard = dashboards.Single();
                var roleNamesArray = RoleNames
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToArray();

                var dashboardName = dashboard.Name;
                var newFormxml = SetRolesOnFormXml(dashboard.FormXml, roleNamesArray);
                if (dashboard.FormXml == newFormxml)
                {
                    WriteVerbose("Exiting without any change..");
                    return;
                }

                WriteVerbose("Saving changes..");
                OrganizationService.Update(new SystemForm
                {
                    Id = dashboard.Id,
                    FormXml = newFormxml
                });
                WriteVerbose("Publishing changes..");
                OrganizationService.PublishXml($"<dashboards><dashboard>{dashboard.Id}</dashboard></dashboards>");
            }
        }


        private string SetRolesOnFormXml(string formxml, string[] rolesArray)
        {
            var dashboardXml = XDocument.Parse(formxml);

            var displayConditions = dashboardXml.Descendants("DisplayConditions").SingleOrDefault();
            displayConditions?.Remove();

            if (!rolesArray.Any())
            {
                WriteVerbose("Empty roles array - saving dashboard without DisplayConditions.");
                return formxml;
            }

            var roles = GetRolesByName(rolesArray);
            if (roles.Count != rolesArray.Length)
            {
                WriteWarning($"The list of found roles differs from the input roles array. Please check parameters.");
                return formxml;
            }

            displayConditions = new XElement("DisplayConditions", new XAttribute("FallbackForm", "true"),
                roles.Select(x => new XElement("Role", new XAttribute("Id", x.GetAttributeValue<EntityReference>("parentrootroleid").Id))));
            dashboardXml.Element("form").Add(displayConditions);
            return dashboardXml.ToString(SaveOptions.DisableFormatting);
        }

        private ICollection<Entity> GetRolesByName(string[] roleNames) => OrganizationService.RetrieveMultiple(
            new QueryExpression
            {
                EntityName = "role",
                Distinct = true,
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("name", ConditionOperator.In, roleNames)
                    }
                },
                ColumnSet = new ColumnSet("parentrootroleid", "name")
            }).Entities;

        #endregion
    }
}
