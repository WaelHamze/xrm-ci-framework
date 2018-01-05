using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security;
using System.Configuration;
using Microsoft.Dynamics365.UIAutomation.Browser;
using Microsoft.Dynamics365.UIAutomation.Api;
using System.Collections.Generic;

namespace Xrm.CI.Framework.Sample.UIAutomation
{
    [TestClass]
    public class CreateContactTest
    {
        private readonly SecureString _username = ConfigurationManager.AppSettings["OnlineUsername"].ToSecureString();
        private readonly SecureString _password = ConfigurationManager.AppSettings["OnlinePassword"].ToSecureString();
        private readonly Uri _xrmUri = new Uri(ConfigurationManager.AppSettings["OnlineCrmUrl"].ToString());


        private readonly BrowserOptions _options = new BrowserOptions
        {
            BrowserType = BrowserType.Chrome,
            PrivateMode = true,
            FireEvents = true
        };

        [TestMethod]
        public void CreateNewContact()
        {
            using (var xrmBrowser = new XrmBrowser(_options))
            {
                xrmBrowser.LoginPage.Login(_xrmUri, _username, _password);
                xrmBrowser.GuidedHelp.CloseGuidedHelp();

                xrmBrowser.ThinkTime(500);
                xrmBrowser.Navigation.OpenSubArea("Sales", "Contacts");

                xrmBrowser.ThinkTime(2000);
                xrmBrowser.Grid.SwitchView("Active Contacts");

                xrmBrowser.ThinkTime(1000);
                xrmBrowser.CommandBar.ClickCommand("New");

                xrmBrowser.ThinkTime(5000);

                var fields = new List<Field>
                {
                    new Field() {Id = "firstname", Value = "Test"},
                    new Field() {Id = "lastname", Value = "Contact"}
                };
                xrmBrowser.Entity.SetValue(new CompositeControl() { Id = "fullname", Fields = fields });
                xrmBrowser.Entity.SetValue("emailaddress1", "test@contoso.com");
                xrmBrowser.Entity.SetValue("mobilephone", "555-555-5555");
                xrmBrowser.Entity.SetValue("birthdate", DateTime.Parse("11/1/1980"));
                xrmBrowser.Entity.SetValue(new OptionSet { Name = "preferredcontactmethodcode", Value = "Email" });

                xrmBrowser.CommandBar.ClickCommand("Save");
                xrmBrowser.ThinkTime(5000);

            }
        }
    }
}
