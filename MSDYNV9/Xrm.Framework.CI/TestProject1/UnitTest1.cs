using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.PowerShell.Cmdlets;
using Xunit.Abstractions;

namespace TestProject1
{
    public class UnitTest1(ITestOutputHelper _output)
    {

        public class GetXrmPluginRegistrationFromAssemblyCopy
        {
            public string AssemblyPath { get; set; }
            public string MappingFile { get; set; }
            public bool? MaintainIdsAndSteps { get; set; }
            public string CustomAttributeClassName { get; set; }

            public void ProcessRecord(ITestOutputHelper _output)
            {
                if (string.IsNullOrEmpty(AssemblyPath) || !File.Exists(AssemblyPath))
                    throw new Exception($"{nameof(AssemblyPath)} must exist");

                if (string.IsNullOrEmpty(MappingFile) || !File.Exists(MappingFile))
                    throw new Exception($"{nameof(MappingFile)} must exist");

                if (string.IsNullOrEmpty(CustomAttributeClassName))
                    CustomAttributeClassName = "XrmCiFramework.XrmCiPluginRegistration";

                var pluginRegistrationHelper = new PluginRegistrationHelper((message) => _output.WriteLine(message), (message) => _output.WriteLine($"WARNING: {message}"));

                var assembly =
                    pluginRegistrationHelper.GetPluginRegistrationObject(AssemblyPath, CustomAttributeClassName) as Assembly;

                if (!MaintainIdsAndSteps.HasValue || (MaintainIdsAndSteps.HasValue && MaintainIdsAndSteps == true))
                {
                    var assemblyFromMappingFile = pluginRegistrationHelper.ReadMappingFile(MappingFile);
                    if (assemblyFromMappingFile != null)
                        assembly += assemblyFromMappingFile;
                }

                pluginRegistrationHelper.SerializerObjectToFile(MappingFile, assembly);
            }
        }

        [Fact]
        public void GetXrmPluginRegistrationFromAssemblyCommand_Test()
        {
            //arrange
            var cmd = new GetXrmPluginRegistrationFromAssemblyCopy
            {
                AssemblyPath = @"C:\Users\ETRO\source\repos\_private\_MyAppsInPowerApps-Private\Dataverse\Assemblies\MyApps.Dataverse.MyComics\MyApps.Dataverse.MyComics.Plugins\bin\Debug\net462\publish\MyApps.Dataverse.MyComics.Plugins.dll",
                // CustomAttributeClassName = "", //gets default
                MaintainIdsAndSteps = true,
                MappingFile = @"C:\Users\ETRO\source\repos\_private\_MyAppsInPowerApps-Private\Dataverse\Assemblies\MyApps.Dataverse.MyComics\MyApps.Dataverse.MyComics.Plugins\PluginRegistration.json"
            };

            //act
            cmd.ProcessRecord(_output);
        }
    }
}