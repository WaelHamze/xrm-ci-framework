using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Xrm.Framework.CI.Common
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

        public static void PublishXml(this IOrganizationService service, string innerImportXml) => service.Execute(new PublishXmlRequest
        {
            ParameterXml = $"<importexportxml>{innerImportXml}</importexportxml>"
        });

        public static AttributeMetadata GetAttributeMetadata(this IOrganizationService service, string entityName, string attributeName) =>
            ((RetrieveAttributeResponse) service.Execute(new RetrieveAttributeRequest()
            {
                EntityLogicalName = entityName,
                LogicalName = attributeName
            })).AttributeMetadata;

        public static void DeleteOptionValue(this IOrganizationService service, string entityName, string attributeName, int value) =>
            service.Execute(new DeleteOptionValueRequest()
            {
                EntityLogicalName = entityName,
                AttributeLogicalName = attributeName,
                Value = value
            });

        public static void DeleteOptionValue(this IOrganizationService service, string optionSetName, int value) =>
            service.Execute(new DeleteOptionValueRequest()
            {
                OptionSetName = optionSetName,
                Value = value
            });
    }
}
