using System;
using System.Collections.Generic;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public class Step
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public string MessageName { get; set; }

        public string Description { get; set; }

        public string CustomConfiguration { get; set; }

        public string FilteringAttributes { get; set; }

        public string ImpersonatingUserFullname { get; set; }

        public SdkMessageProcessingStep_Mode? Mode { get; set; }

        public string PrimaryEntityName { get; set; }

        public int? Rank { get; set; }

        public SdkMessageProcessingStep_Stage? Stage { get; set; }

        public SdkMessageProcessingStep_SupportedDeployment? SupportedDeployment { get; set; }

        public List<Image> Images { get; set; }

        public override int GetHashCode()
        {
            var hashCode = -823040870;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MessageName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CustomConfiguration);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FilteringAttributes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ImpersonatingUserFullname);
            hashCode = hashCode * -1521134295 + EqualityComparer<SdkMessageProcessingStep_Mode?>.Default.GetHashCode(Mode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PrimaryEntityName);
            hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(Rank);
            hashCode = hashCode * -1521134295 + EqualityComparer<SdkMessageProcessingStep_Stage?>.Default.GetHashCode(Stage);
            hashCode = hashCode * -1521134295 + EqualityComparer<SdkMessageProcessingStep_SupportedDeployment?>.Default.GetHashCode(SupportedDeployment);
            return hashCode;
        }
    }
}
