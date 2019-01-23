using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.Xrm.Sdk.Metadata;

using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Removes the given option from specified OptionSet.</para>
    /// <para type="description">The Remove-XrmOption cmdlet removes an value from specified local or global OptionSet.</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Remove-XrmOption -EntityName "new_entity" -AttributeName "new_attribute" -OptionValue 10</code>
    ///   <code>C:\PS>Remove-XrmOption -EntityName "new_entity" -AttributeName "new_attribute" -OptionLabel "Option x"</code>
    ///   <para>Removes option specified by Value or by Label from local or global OptionSet specified by entity and attribute name</para>
    /// </example>
    /// <para type="link" uri="https://docs.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.messages.deleteoptionvaluerequest?view=dynamics-general-ce-9">DeleteOptionValueRequest.</para>
    [Cmdlet(VerbsCommon.Remove, "XrmOption")]
    public class RemoveXrmOptionCommand: XrmCommandBase
    {
        #region Parameters

        private const string findByValue = "FindByValue";
        private const string findByLabel = "FindByLabel";

        /// <summary>
        /// <para type="description">The logical name of the entity.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string EntityName { get; set; }

        /// <summary>
        /// <para type="description">The logical name of the attribute.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string AttributeName { get; set; }

        /// <summary>
        /// <para type="description">Value of option to remove.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = findByValue)]
        public int OptionValue { get; set; }

        /// <summary>
        /// <para type="description">Label of option to remove.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = findByLabel)]
        public string OptionLabel { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            string filteringKey;
            string filteringValue;
            Func<OptionMetadata, bool> predicateFunction;

            if (ParameterSetName == findByValue)
            {
                filteringKey = "value";
                filteringValue = OptionValue.ToString();
                predicateFunction = x => x.Value == OptionValue;
            }
            else
            {
                filteringKey = "label";
                filteringValue = OptionLabel;
                predicateFunction = x => x.Label.UserLocalizedLabel?.Label == OptionLabel;
            }

            WriteVerbose($"Trying to remove picklist option on {EntityName}.{AttributeName} with {filteringKey} = {filteringValue}");

            var picklistMetadata = GetPicklistMetadata(EntityName, AttributeName);
            if (picklistMetadata == null)
            {
                return;
            }

            var optionMetadata = picklistMetadata.OptionSet.Options.FirstOrDefault(predicateFunction);
            if (optionMetadata == null)
            {
                WriteVerbose($"Couldn't find option with {filteringKey} = {filteringValue}");
                return;
            }
            WriteVerbose($"Found option: {optionMetadata.Label.UserLocalizedLabel?.Label} / {optionMetadata.Value}");

            string publishXml;
            if (picklistMetadata.OptionSet.IsGlobal == true)
            {
                WriteVerbose($"Removing value from global optionset: {picklistMetadata.OptionSet.Name}");
                OrganizationService.DeleteOptionValue(picklistMetadata.OptionSet.Name, optionMetadata.Value.Value);
                publishXml = $"<optionsets><optionset>{picklistMetadata.OptionSet.Name}</optionset></optionsets>";
            }
            else
            {
                WriteVerbose($"Removing value from local optionset: {EntityName}.{AttributeName}");
                OrganizationService.DeleteOptionValue(EntityName, AttributeName, optionMetadata.Value.Value);
                publishXml = $"<entities><entity>{EntityName}</entity></entities>";
            }
            WriteVerbose($"Publishing changes..");
            OrganizationService.PublishXml(publishXml);
            WriteVerbose($"Option removed.");
        }

        private PicklistAttributeMetadata GetPicklistMetadata(string entityName, string attributeName)
        {
            var attributeMetadata = OrganizationService.GetAttributeMetadata(entityName, attributeName);
            if (!(attributeMetadata is PicklistAttributeMetadata))
            {
                WriteWarning($"Passed attribute {entityName}.{attributeName} is {attributeMetadata.AttributeTypeName.Value} but PicklistType expected.");
            }

            return attributeMetadata as PicklistAttributeMetadata;
        }
        #endregion
    }
}