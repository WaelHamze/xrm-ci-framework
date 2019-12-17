using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Data;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets;

namespace Xrm.Resco.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves the document template from CRM</para>
    /// <para type="description">This cmdlet retrieves the document template from CRM by name and saves as Word/Excel document with metadata in xml file</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Get-XrmDocumentTemplate -ConnectionString "" -Name "My template name"</code>
    ///   <para>Retrieves the "My template name" template from CRM</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "XrmDocumentTemplate")]
    public class GetXrmDocumentTemplateCommand : XrmCommandBase
    {
        /// <summary>
        /// <para type="description">The unique name of the document template to be retrieved.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            WriteVerbose($"Retrieving Document Template: {Name}");

            using (var context = new CIContext(OrganizationService))
            {
                var template = context.DocumentTemplateSet.FirstOrDefault(x => x.Name == Name);

                if (template == null)
                {
                    throw new ItemNotFoundException($"Document template with name {Name} not found");
                }

                var etc = OrganizationService.GetEntityTypeCode(template.AssociatedEntityTypeCode);
                var metadata = DocumentTemplateMetadata.FromDocumentTemplate(template, etc);
                Serializers.SaveXml($"{template.Name}.xml", metadata);
                File.WriteAllBytes($"{template.Name}.{(DocumentTemplateExtension)template.DocumentTypeEnum}", Convert.FromBase64String(template.Content));
            }
        }
    }
}
