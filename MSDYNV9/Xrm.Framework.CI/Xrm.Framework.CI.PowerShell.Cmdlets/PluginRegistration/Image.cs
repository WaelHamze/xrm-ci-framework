using System;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public class Image
    {
        public Guid? Id { get; set; }

        public string Attributes { get; set; }

        public string EntityAlias { get; set; }

        public string MessagePropertyName { get; set; }

        public SdkMessageProcessingStepImage_ImageType? ImageType { get; set; }
    }
}
