using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;
using System.Linq;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Publishes a Dynamics Theme</para>
    /// <para type="description">The Publish-XrmTheme cmdlet publishes a Dynamics Theme.
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Publish-XrmTheme -ThemeId $themeId</code>
    ///   <para>Publishes a theme.</para>
    /// </example>
    /// <para type="link" uri="https://docs.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.publishthemerequest?view=dynamics-general-ce-9">PublishTheme.</para>
    [Cmdlet(VerbsCommon.Get, "XrmTheme")]
    public class GetXrmThemeCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The Id of the Theme to publish.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public Guid? ThemeId { get; set; }

        [Parameter(Mandatory = false)]
        public string ThemeName { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (ThemeId == null && ThemeName == null)
            {
                throw new Exception("ThemeId or ThemeName not provided");
            }

            if (ThemeId != null && ThemeName != null)
            {
                throw new Exception("Only 1 of ThemeId or ThemeName can be specified");
            }

            if (ThemeName != null)
            {
                //query for the theme to get the Id
                var q = new QueryExpression("theme");
                q.Criteria.AddCondition("name", ConditionOperator.Equal, ThemeName);

                var entities = OrganizationService.RetrieveMultiple(q);
                if (!entities.Entities.Any())
                {
                    throw new Exception("Could not locate theme by name");
                }

                ThemeId = entities.Entities.First().Id;

            }

            var theme = OrganizationService.Retrieve("theme", ThemeId.Value, new ColumnSet(true));

            base.WriteVerbose(string.Format("Exporting Theme"));

            var themeInfo = new XrmThemeInfo();
            themeInfo.Name = theme.GetAttributeValue<string>("name");
            themeInfo.Id = theme.Id;
            if (theme.Contains("logoid"))
            {
                themeInfo.LogoId = theme.GetAttributeValue<EntityReference>("logoid").Id ;
            }

            WriteObject(themeInfo);

            base.WriteVerbose(string.Format("Theme Published"));
        }

        #endregion
    }
}