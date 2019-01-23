using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Updates a Web Resource.</para>
    /// <para type="description">The Set-WebResource cmdlet updates an existing Web Resource in CRM.
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Set-WebResource -Path $path</code>
    ///   <para>Updates a Web Resource.</para>
    /// </example>
    /// <para type="link" uri="http://msdn.microsoft.com/en-us/library/microsoft.xrm.sdk.messages.updaterequest.aspx">UpdateRequest.</para>
    [Cmdlet(VerbsCommon.Set, "XrmDashboardRole")]
    public class SetXrmDashboardRoleCommand : XrmCommandBase
    {
        #region Parameters

        private const string findByName = "FindByName";
        private const string findById = "FindById";
        private const int Dashboard = 0;

        /// <summary>
        /// <para type="description">The full path to the web resource file. e.g. C:\Solution\WebResources\Test.js</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = findByName)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The full path to the web resource file. e.g. C:\Solution\WebResources\Test.js</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = findById)]
        public Guid Id { get; set; }

        /// <summary>
        /// <para type="description">The unique name of the web resource in CRM. e.g. prefix_Test.js</para>
        /// </summary>
        [Parameter(Mandatory = true)]
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

            Entity dashboard;

            if (ParameterSetName == findById)
            {
                dashboard = OrganizationService.Retrieve("systemform", Id, new ColumnSet("formxml", "type", "name"));
            }
            else
            {
                var results = FindDashboardByName(Name);
                if (results.Count == 0)
                {
                    WriteWarning($"Couldn't find dashboard with name {Name}");
                    return;
                }

                if (results.Count > 1)
                {
                    WriteWarning($"There are more than one dashboard with name {Name}. Try execute cmdlet using Id.");
                    return;
                }

                dashboard = results.Single();
            }

            var roleNamesArray = RoleNames
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();

            var oldFormXml = dashboard.GetAttributeValue<string>("formxml");
            var dashboardName = dashboard.GetAttributeValue<string>("name");
            var newFormxml = SetRolesOnFormXml(oldFormXml, roleNamesArray);

            if (oldFormXml == newFormxml)
            {
                WriteVerbose("Exiting without any change..");
                return;
            }

            WriteVerbose("Saving changes..");
            OrganizationService.Update(new Entity
            {
                Id = dashboard.Id,
                LogicalName = dashboard.LogicalName,
                Attributes =
                {
                    {"formxml", newFormxml}
                }
            });
            WriteVerbose("Publishing changes..");
            OrganizationService.PublishXml($"<dashboards><dashboard>{dashboard.Id}</dashboard></dashboards>");
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

        private ICollection<Entity> FindDashboardByName(string name) => OrganizationService.RetrieveMultiple(
            new QueryByAttribute
            {
                EntityName = "systemform",
                Attributes = {"name", "type"},
                Values = {name, Dashboard},
                ColumnSet = new ColumnSet("formxml", "type", "name")
            }).Entities;

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
