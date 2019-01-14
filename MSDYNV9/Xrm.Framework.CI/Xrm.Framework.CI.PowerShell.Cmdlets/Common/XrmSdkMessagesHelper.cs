using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Xrm.Framework.CI.PowerShell.Cmdlets.Common
{
    public static class XrmSdkMessagesHelper
    {
        public static int[] GetInstalledLanguages(this IOrganizationService service) => ((RetrieveInstalledLanguagePacksResponse)service.Execute(new RetrieveInstalledLanguagePacksRequest())).RetrieveInstalledLanguagePacks;

        public static int[] GetProvisionedLanguages(this IOrganizationService service) => ((RetrieveProvisionedLanguagesResponse)service.Execute(new RetrieveProvisionedLanguagesRequest())).RetrieveProvisionedLanguages;

        public static EntityMetadata GetEntityMetadata(this IOrganizationService service, string logicalname) => ((RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest {
            LogicalName = logicalname,
            EntityFilters = EntityFilters.Entity
        })).EntityMetadata;

        public static void ProvisionLanguage(this IOrganizationService service, int language) => service.Execute(new ProvisionLanguageRequest
        {
            Language = language
        });
    }
}