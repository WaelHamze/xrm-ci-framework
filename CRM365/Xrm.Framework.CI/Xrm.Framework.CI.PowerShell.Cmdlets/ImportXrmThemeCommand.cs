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
    [Cmdlet(VerbsData.Import, "XrmTheme")]
    public class ImportXrmThemeCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The Id of the Theme to publish.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public XrmThemeInfo Theme { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();


            var newTheme = new Entity("theme");
            newTheme.Id = Theme.Id;
            newTheme["name"] = Theme.Name;
            newTheme["headercolor"] = Theme.HeaderColor;
            newTheme["hoverlinkeffect"] = Theme.HoverLinkEffect;
            newTheme["logoid"] = new EntityReference("webresource", Theme.LogoId);
            newTheme["logotooltip"] = Theme.LogoToolTip;
            newTheme["maincolor"] = Theme.MainColor;
            newTheme["navbarbackgroundcolor"] = Theme.NavBarBackgroundColor;
            newTheme["navbarshelfcolor"] = Theme.NavBarShelfColor;
            newTheme["pageheaderbackgroundcolor"] = Theme.PageHeaderBackgroundColor;
            newTheme["panelheaderbackgroundcolor"] = Theme.PanelHeaderBackgroundColor;
            newTheme["processcontrolcolor"] = Theme.ProcessControlColor;
            newTheme["selectedlinkeffect"] = Theme.SelectedLinkEffect;
            newTheme["accentcolor"] = Theme.AccentColor;
            newTheme["backgroundcolor"] = Theme.BackgroundColor;
            newTheme["controlborder"] = Theme.ControlBorder;
            newTheme["controlshade"] = Theme.ControlShade;
            newTheme["defaultcustomentitycolor"] = Theme.DefaultCustomEntityColor;
            newTheme["defaultentitycolor"] = Theme.DefaultEntityColor;
            newTheme["globallinkcolor"] = Theme.GlobalLinkColor;

            bool exists = false;
            try
            {
                var existsTheme = OrganizationService.Retrieve("theme", Theme.Id, new ColumnSet(true));
                exists = true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Does Not Exist"))
                {
                    base.WriteVerbose(string.Format("Creating new Theme"));
                    OrganizationService.Create(newTheme);
                }
            }

            if (exists)
            {
                base.WriteVerbose(string.Format("Updating existing Theme"));
                OrganizationService.Update(newTheme);
            }


            base.WriteVerbose(string.Format("Theme Imported"));
        }

        #endregion
    }
}