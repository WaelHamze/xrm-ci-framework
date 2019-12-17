using System;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.Common.Data
{
    public class DocumentTemplateMetadata
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DocumentTemplateExtension DocumentType { get; set; }
        public Guid Id { get; set; }
        public int? Etc { get; set; }
        public string Etn { get; set; }
        public int? LanguageCode { get; set; }

        public DocumentTemplate ToDocumentTemplate() =>
            new DocumentTemplate
            {
                Id = Id,
                Name = Name,
                Description = Description,
                DocumentTypeEnum = (DocumentTemplate_DocumentType)DocumentType,
                LanguageCode = LanguageCode,
                AssociatedEntityTypeCode = Etn,
            };

        public static DocumentTemplateMetadata FromDocumentTemplate(DocumentTemplate template, int? etc) =>
            new DocumentTemplateMetadata
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                DocumentType = (DocumentTemplateExtension)template.DocumentTypeEnum,
                Etn = template.AssociatedEntityTypeCode,
                Etc = etc,
                LanguageCode = template.LanguageCode
            };
    }
}
