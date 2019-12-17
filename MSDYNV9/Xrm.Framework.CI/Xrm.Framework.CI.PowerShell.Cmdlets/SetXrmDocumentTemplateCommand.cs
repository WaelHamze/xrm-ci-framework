using System;
using System.IO;
using System.Management.Automation;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Data;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets;

namespace Xrm.Resco.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Upsert the document template into CRM</para>
    /// <para type="description">This cmdlet upserts the document template into CRM</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Get-XrmDocumentTemplate -ConnectionString "" -MetadataFilename "My template name.xml"</code>
    ///   <para>Upsert the "My template name" document template into CRM</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "XrmDocumentTemplate")]
    public class SetXrmDocumentTemplateCommand : XrmCommandBase
    {
        /// <summary>
        /// <para type="description">The path to metadata file with information about the document template to be inserted/updated.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string MetadataFilename { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!File.Exists(MetadataFilename))
            {
                throw new ItemNotFoundException($"Document template metadata with filename {MetadataFilename} not found");
            }

            var metadata = Serializers.ParseXml<DocumentTemplateMetadata>(MetadataFilename);
            var documentFilename = Path.ChangeExtension(MetadataFilename, metadata.DocumentType.ToString());

            if (!File.Exists(documentFilename))
            {
                throw new ItemNotFoundException($"Document template with filename {documentFilename} not found");
            }
            
            WriteVerbose($"Uploading Document Template: {metadata.Name}");

            using (var context = new CIContext(OrganizationService))
            {
                var template = metadata.ToDocumentTemplate();
                template.Content = Convert.ToBase64String(File.ReadAllBytes(documentFilename));

                if (metadata.DocumentType == DocumentTemplateExtension.docx)
                {
                    var newEtc = base.OrganizationService.GetEntityTypeCode(metadata.Etn);

                    if (newEtc != metadata.Etc)
                    {
                        WriteVerbose($"ReRouting entity type code for {metadata.Etn} from {metadata.Etc} to {newEtc}");
                        template.Content = TemplateManager.ReRouteEtcViaOpenXML(template.Content, metadata.Etn, metadata.Etc, newEtc);
                    }
                }

                OrganizationService.Upsert(template);
                WriteVerbose($"Upsert completed");
            }
        }
    }
}
