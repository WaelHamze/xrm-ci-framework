using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.PowerShell.Cmdlets.Test.PluginRegistration
{
    [TestFixture]
    public class StepTest
    {
        #region SetUp
        private Step _stepLeft;
        private Step _stepRight;
        private Image _imageLeft;
        private Image _imageRight;

        [SetUp]
        public void SetUp()
        {
            _stepLeft = new Step
            {
                Id = Guid.NewGuid(),
                Name = "NameLeft",
                PrimaryEntityName = "contact",
                Stage = SdkMessageProcessingStep_Stage.Preoperation,
                CustomConfiguration = "cc left",
                FilteringAttributes = "filter left",
                ImpersonatingUserFullname = "mo left",
                Mode = SdkMessageProcessingStep_Mode.Synchronous,
                Rank = 0,
                SupportedDeployment = SdkMessageProcessingStep_SupportedDeployment.ServerOnly,
                AsyncAutoDelete = false,
                StateCode = SdkMessageProcessingStepState.Enabled
            };

            _stepRight = new Step
            {
                Id = Guid.NewGuid(),
                Name = "NameRight",
                PrimaryEntityName = "account",
                Stage = SdkMessageProcessingStep_Stage.Prevalidation,
                CustomConfiguration = "cc right",
                FilteringAttributes = "filter right",
                ImpersonatingUserFullname = "mo right",
                Mode = SdkMessageProcessingStep_Mode.Asynchronous,
                Rank = 1,
                SupportedDeployment = SdkMessageProcessingStep_SupportedDeployment.Both,
                AsyncAutoDelete = false,
                StateCode = SdkMessageProcessingStepState.Enabled
            };

            _imageLeft = new Image
            {
                Id = Guid.NewGuid(),
                Attributes = "att left",
                EntityAlias = "Alias",
                MessagePropertyName = "MPN",
                ImageType = SdkMessageProcessingStepImage_ImageType.PreImage
            };

            _stepLeft.Images = new List<Image>()
            {
                _imageLeft, new Image
                {
                    Id = Guid.NewGuid(),
                    Attributes = "att left2",
                    EntityAlias = "Alias2",
                    MessagePropertyName = "MPN2",
                    ImageType = SdkMessageProcessingStepImage_ImageType.PostImage
                }
            };
            
            _imageRight = new Image
            {
                Id = Guid.NewGuid(),
                Attributes = "att left",
                EntityAlias = "Alias",
                MessagePropertyName = "MPN",
                ImageType = SdkMessageProcessingStepImage_ImageType.PreImage
            };

            _stepRight.Images = new List<Image>() {_imageRight};
        }
        #endregion

        #region Tests
        [Test]
        public void Addition_Operator_Imposes_Rhs_Id()
        {
            var addedStep = _stepLeft + _stepRight;
            Assert.AreEqual(addedStep.Id, _stepRight.Id);
        }

        [Test]
        public void Addition_Operator_Takes_Other_Lhs_Values()
        {
            var addedStep = _stepLeft + _stepRight;
            Assert.AreEqual(addedStep.Name, _stepLeft.Name);
            Assert.AreEqual(addedStep.PrimaryEntityName, _stepLeft.PrimaryEntityName);
            Assert.AreEqual(addedStep.CustomConfiguration, _stepLeft.CustomConfiguration);
            Assert.AreEqual(addedStep.FilteringAttributes, _stepLeft.FilteringAttributes);
            Assert.AreEqual(addedStep.ImpersonatingUserFullname, _stepLeft.ImpersonatingUserFullname);
            Assert.AreEqual(addedStep.Mode, _stepLeft.Mode);
            Assert.AreEqual(addedStep.Rank, _stepLeft.Rank);
            Assert.AreEqual(addedStep.SupportedDeployment, _stepLeft.SupportedDeployment);
            Assert.AreEqual(addedStep.AsyncAutoDelete, _stepLeft.AsyncAutoDelete);
            Assert.AreEqual(addedStep.StateCode, _stepLeft.StateCode);
        }

        [Test]
        public void Addition_Operator_Keeps_Lhs_Images_With_No_Rhs_Counterpart()
        {
            var addedStep = _stepLeft + _stepRight;
            Assert.AreEqual(2, addedStep.Images.Count);
        }

        [Test]
        public void Addition_Operator_Adds_Images()
        {
            var addedStep = _stepLeft + _stepRight;
            Assert.IsNotNull(addedStep.Images.FirstOrDefault(x => x.Id == _imageRight.Id));
            Assert.IsNull(addedStep.Images.FirstOrDefault(x => x.Id == _imageLeft.Id));
        }
        #endregion
    }
}