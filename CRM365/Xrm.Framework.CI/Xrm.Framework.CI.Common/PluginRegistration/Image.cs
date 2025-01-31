using System;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.Common
{
    public class Image
    {
        public Guid? Id { get; set; }

        public string Attributes { get; set; }

        public string EntityAlias { get; set; }

        public string MessagePropertyName { get; set; }

        public SdkMessageProcessingStepImage_ImageType? ImageType { get; set; }

        public static Image operator +(Image b, Image c)
        {
            return new Image
            {
                Id = c.Id,
                Attributes = b.Attributes,
                EntityAlias = b.EntityAlias,
                MessagePropertyName = b.MessagePropertyName,
                ImageType = b.ImageType
            };
        }
    }

    public static class ImageExtensions
    {
        public static bool SameAsRegistered(this Image original, Image compare)
        {
            return original != null
                   && (compare != null
                       && original.EntityAlias == compare.EntityAlias
                       && original.MessagePropertyName == compare.MessagePropertyName
                       && original.ImageType == compare.ImageType);
        }
    }
}
