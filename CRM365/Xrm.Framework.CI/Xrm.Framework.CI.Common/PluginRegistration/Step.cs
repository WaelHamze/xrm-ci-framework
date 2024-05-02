using System;
using System.Collections.Generic;
using System.Linq;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.Common
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

        public bool? AsyncAutoDelete { get; set; }

        public SdkMessageProcessingStepState? StateCode { get; set; }

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
            hashCode = hashCode * -1521134295 + EqualityComparer<bool?>.Default.GetHashCode(AsyncAutoDelete);
            return hashCode;
        }

        public static Step operator +(Step b, Step c)
        {
            var step = new Step
            {
                Id = c.Id,
                Name = b.Name,
                MessageName = b.MessageName,
                Description = b.Description,
                CustomConfiguration = b.CustomConfiguration,
                FilteringAttributes = b.FilteringAttributes,
                ImpersonatingUserFullname = b.ImpersonatingUserFullname,
                Mode = b.Mode,
                PrimaryEntityName = b.PrimaryEntityName,
                Rank = b.Rank,
                Stage = b.Stage,
                SupportedDeployment = b.SupportedDeployment,
                AsyncAutoDelete = b.AsyncAutoDelete,
                StateCode = b.StateCode,
                Images = new List<Image>()
            };

            if (b.Images == null) return step;

            foreach (var image in b.Images)
            {
                var original = b.Images.First(x => x.SameAsRegistered(image));
                var corresponding = c.Images?.FirstOrDefault(x => x.SameAsRegistered(image));
                if (corresponding != null)
                {
                    original += corresponding;
                }
                step.Images.Add(original);
            }

            return step;
        }
    }

    public static class StepExtensions
    {
        public static bool SameAsRegistered(this Step original, Step compare)
        {
            return original != null
                   && (compare != null
                       && original.Name == compare.Name
                       && original.PrimaryEntityName == compare.PrimaryEntityName
                       && original.Stage == compare.Stage);
        }
    }

}
