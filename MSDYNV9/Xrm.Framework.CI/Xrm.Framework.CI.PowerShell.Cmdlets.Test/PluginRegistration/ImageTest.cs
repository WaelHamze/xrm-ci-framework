using System;
using NUnit.Framework;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets.Test.PluginRegistration
{
    [TestFixture]
    public class ImageTest
    {
        #region SetUp
        private Image _imageLeft;
        private Image _imageRight;

        [SetUp]
        public void SetUp()
        {
            _imageLeft = new Image
            {
                Id = Guid.NewGuid(),
                Attributes = "Attributes Left",
                EntityAlias = "Alias Left",
                MessagePropertyName = "MPN Left",
                ImageType = SdkMessageProcessingStepImage_ImageType.PreImage
            };

            _imageRight = new Image
            {
                Id = Guid.NewGuid(),
                Attributes = "Attributes Right",
                EntityAlias = "Alias Right",
                MessagePropertyName = "MPN Right",
                ImageType = SdkMessageProcessingStepImage_ImageType.PostImage
            };
        }
        #endregion

        #region Tests
        [Test]
        public void Addition_Operator_Imposes_Rhs_Id()
        {
            var addedImage = _imageLeft + _imageRight;
            Assert.AreEqual(addedImage.Id, _imageRight.Id);
        }

        [Test]
        public void Addition_Operator_Takes_Other_Lhs_Values()
        {
            var addedImage = _imageLeft + _imageRight;
            Assert.AreEqual(addedImage.Attributes, _imageLeft.Attributes);
            Assert.AreEqual(addedImage.EntityAlias, _imageLeft.EntityAlias);
            Assert.AreEqual(addedImage.MessagePropertyName, _imageLeft.MessagePropertyName);
            Assert.AreEqual(addedImage.ImageType, _imageLeft.ImageType);
        }
        #endregion
    }
}