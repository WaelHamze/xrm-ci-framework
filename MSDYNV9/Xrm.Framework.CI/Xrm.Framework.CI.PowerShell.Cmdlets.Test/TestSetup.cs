using FakeXrmEasy;
using Microsoft.Xrm.Sdk;
using NUnit.Framework;
using System;
using System.Management.Automation.Runspaces;

namespace Xrm.Framework.CI.PowerShell.Cmdlets.Test
{
    public static class TestData
    {
        public static XrmFakedContext XrmFakedContext = new XrmFakedContext();
        public static Runspace Runspace;
    }
    public class SetXrmWebResourcesFromFolderFake : SetXrmWebResourcesFromFolder
    {
        protected override void BeginProcessing()
        {
        }

        protected override IOrganizationService OrganizationService => TestData.XrmFakedContext.GetOrganizationService();
    }

    [SetUpFixture]
    public class TestSetup
    {
        RunspaceConfiguration config = RunspaceConfiguration.Create();
        Runspace runspace;

        [OneTimeSetUp]
        public void Setup()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
            config.Cmdlets.Append(
                new CmdletConfigurationEntry("Set-XrmWebResourcesFromFolder", typeof(SetXrmWebResourcesFromFolderFake), ""));
            runspace = RunspaceFactory.CreateRunspace(config);
            runspace.Open();
            TestData.Runspace = runspace;
        }

        [OneTimeTearDown]
        public void TearDown() => runspace?.Close();


    }
}
