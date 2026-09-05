using FakeItEasy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;
using C = Xrm.Framework.CI.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets.Test
{
    [TestFixture]
    public class PluginRegistrationObjectFactoryTest
    {
        #region Setup
        private List<Dictionary<string, object>> _argsList;
        private IReflectionLoader _fakeReflectionLoader;

        [SetUp]
        public void SetUp()
        {
            PopulateArgs();
            SetupFakeReflectionLoader();
        }

        private void SetupFakeReflectionLoader()
        {
            _fakeReflectionLoader = A.Fake<IReflectionLoader>();
            A.CallTo(() => _fakeReflectionLoader.AssemblyName).Returns("MyNameSpace.MyAssembly.dll");
            A.CallTo(() => _fakeReflectionLoader.Constructors).Returns(new List<Constructor> { new Constructor { ClassName = "TestClass1", ArgsList = _argsList } });
        }

        private void PopulateArgs(string message = "Update")
        {
            if (_argsList == null)
            {
                _argsList = new List<Dictionary<string, object>>();
            }

            _argsList.Add(new Dictionary<string, object>
            {
                {"isolationMode", PluginAssembly_IsolationMode.Sandbox},
                {"sourceType", PluginAssembly_SourceType.Database},
                {"description", "test type description"},
                {"workflowGroupName", ""},
                {"customConfiguration", "custom config"},
                {"filters", "firstname,lastname"},
                {"impersonatingUser", "Mo G"},
                {"message", message},
                {"entityLogicalName", "contact"},
                {"executionMode", SdkMessageProcessingStep_Mode.Synchronous},
                {"order", 1},
                {"stage", SdkMessageProcessingStep_Stage.Postoperation.ToString()},
                {"supportedDeployment", SdkMessageProcessingStep_SupportedDeployment.ServerOnly},
                {"image1Attributes", "im1attr1,im1attr2"},
                {"image1Type", SdkMessageProcessingStepImage_ImageType.PreImage},
                {"image2Attributes", "im2attr1,im2attr2"},
                {"image2Type", SdkMessageProcessingStepImage_ImageType.PostImage},
                {"deleteAsyncOperation", false},
                {"state", SdkMessageProcessingStepState.Enabled}
            });
        }
        #endregion

        #region Tests
        [Test]
        public void GetAssembly_Populates_Assembly_Fields()
        {
            IPluginRegistrationObjectFactory pluginRegistrationObjectFactory = new PluginRegistrationObjectFactory();
            var assembly = pluginRegistrationObjectFactory.GetAssembly(_fakeReflectionLoader);
            Assert.IsInstanceOf<Assembly>(assembly);
            Assert.IsInstanceOf<Guid>(assembly.Id);
            Assert.NotNull(assembly.Id);
            Assert.AreEqual("MyNameSpace.MyAssembly.dll", assembly.Name);
            Assert.AreEqual(PluginAssembly_IsolationMode.Sandbox, assembly.IsolationMode);
            Assert.AreEqual(PluginAssembly_SourceType.Database, assembly.SourceType);
        }

        [Test]
        public void GetAssembly_Populates_Type_Fields()
        {
            IPluginRegistrationObjectFactory pluginRegistrationObjectFactory = new PluginRegistrationObjectFactory();
            var assembly = pluginRegistrationObjectFactory.GetAssembly(_fakeReflectionLoader);
            var type = assembly.PluginTypes[0];
            Assert.IsInstanceOf<C.Type>(type);
            Assert.IsInstanceOf<Guid>(type.Id);
            Assert.NotNull(type.Id);
            Assert.AreEqual("TestClass1", type.Name);
            Assert.AreEqual("test type description", type.Description);
            Assert.AreEqual("TestClass1", type.FriendlyName);
            Assert.AreEqual("TestClass1", type.TypeName);
        }

        [Test]
        public void GetAssembly_Populates_Step_Fields()
        {
            IPluginRegistrationObjectFactory pluginRegistrationObjectFactory = new PluginRegistrationObjectFactory();
            var assembly = pluginRegistrationObjectFactory.GetAssembly(_fakeReflectionLoader);
            var step = assembly.PluginTypes[0].Steps[0];
            Assert.IsInstanceOf<Step>(step);
            Assert.IsInstanceOf<Guid>(step.Id);
            Assert.NotNull(step.Id);
            Assert.AreEqual("TestClass1", step.Name);
            Assert.AreEqual("Postoperation Update of contact", step.Description);
            Assert.AreEqual("Mo G", step.ImpersonatingUserFullname);
            Assert.AreEqual("Update", step.MessageName);
            Assert.AreEqual("contact", step.PrimaryEntityName);
            Assert.AreEqual("firstname,lastname", step.FilteringAttributes);
            Assert.AreEqual(SdkMessageProcessingStep_Mode.Synchronous, step.Mode);
            Assert.AreEqual(1, step.Rank);
            Assert.AreEqual(SdkMessageProcessingStep_SupportedDeployment.ServerOnly, step.SupportedDeployment);
        }


        [Test]
        public void GetAssembly_Populates_Step2_Fields()
        {
            // add a second step
            PopulateArgs("Create");

            // just like GetAssembly_Populates_Step_Fields but now for a second step
            IPluginRegistrationObjectFactory pluginRegistrationObjectFactory = new PluginRegistrationObjectFactory();
            var assembly = pluginRegistrationObjectFactory.GetAssembly(_fakeReflectionLoader);

            bool updateFound = false;
            bool createFound = false;
            foreach (var step in assembly.PluginTypes[0].Steps)
            {
                Assert.IsInstanceOf<Step>(step);
                Assert.IsInstanceOf<Guid>(step.Id);
                Assert.NotNull(step.Id);
                Assert.AreEqual("TestClass1", step.Name);
                Assert.AreEqual("Mo G", step.ImpersonatingUserFullname);
                Assert.AreEqual("contact", step.PrimaryEntityName);
                Assert.AreEqual("firstname,lastname", step.FilteringAttributes);
                Assert.AreEqual(SdkMessageProcessingStep_Mode.Synchronous, step.Mode);
                Assert.AreEqual(1, step.Rank);
                Assert.AreEqual(SdkMessageProcessingStep_SupportedDeployment.ServerOnly, step.SupportedDeployment);

                if (step.MessageName == "Update")
                {
                    updateFound = true;
                    Assert.AreEqual("Postoperation Update of contact", step.Description);
                }
                else if (step.MessageName == "Create")
                {
                    createFound = true;
                    Assert.AreEqual("Postoperation Create of contact", step.Description);
                }
            }

            // make sure both are registered
            Assert.True(updateFound);
            Assert.True(createFound);

            // make sure the id's are not the same
            Assert.AreNotEqual(assembly.PluginTypes[0].Steps[0].Id, assembly.PluginTypes[0].Steps[1].Id);
        }

        [Test]
        public void GetAssembly_Populates_Image1_Fields()
        {
            IPluginRegistrationObjectFactory pluginRegistrationObjectFactory = new PluginRegistrationObjectFactory();
            var assembly = pluginRegistrationObjectFactory.GetAssembly(_fakeReflectionLoader);
            var image = assembly.PluginTypes[0].Steps[0].Images[0];
            Assert.IsInstanceOf<Image>(image);
            Assert.IsInstanceOf<Guid>(image.Id);
            Assert.NotNull(image.Id);
            Assert.AreEqual("im1attr1,im1attr2", image.Attributes);
            Assert.AreEqual(SdkMessageProcessingStepImage_ImageType.PreImage, image.ImageType);
            Assert.AreEqual("PreImage", image.EntityAlias);
            Assert.AreEqual("Target", image.MessagePropertyName);
        }

        [Test]
        public void GetAssembly_Populates_Image2_Fields()
        {
            IPluginRegistrationObjectFactory pluginRegistrationObjectFactory = new PluginRegistrationObjectFactory();
            var assembly = pluginRegistrationObjectFactory.GetAssembly(_fakeReflectionLoader);
            var image = assembly.PluginTypes[0].Steps[0].Images[1];
            Assert.IsInstanceOf<Image>(image);
            Assert.IsInstanceOf<Guid>(image.Id);
            Assert.NotNull(image.Id);
            Assert.AreEqual("im2attr1,im2attr2", image.Attributes);
            Assert.AreEqual(SdkMessageProcessingStepImage_ImageType.PostImage, image.ImageType);
            Assert.AreEqual("PostImage", image.EntityAlias);
            Assert.AreEqual("Target", image.MessagePropertyName);
        }

        [Test]
        public void MessagePropertyName_Set_To_EntityMoniker_For_SetState()
        {
            _argsList[0]["message"] = "SetState";
            IPluginRegistrationObjectFactory pluginRegistrationObjectFactory = new PluginRegistrationObjectFactory();
            var assembly = pluginRegistrationObjectFactory.GetAssembly(_fakeReflectionLoader);
            var image = assembly.PluginTypes[0].Steps[0].Images[1];
            Assert.AreEqual("entityMoniker", image.MessagePropertyName);
        }

        [Test]
        public void MessagePropertyName_Set_To_EntityMoniker_For_SetStateDynamicEntity()
        {
            _argsList[0]["message"] = "SetStateDynamicEntity";
            IPluginRegistrationObjectFactory pluginRegistrationObjectFactory = new PluginRegistrationObjectFactory();
            var assembly = pluginRegistrationObjectFactory.GetAssembly(_fakeReflectionLoader);
            var image = assembly.PluginTypes[0].Steps[0].Images[1];
            Assert.AreEqual("entityMoniker", image.MessagePropertyName);
        }

        [Test]
        public void MessagePropertyName_Set_To_Id_For_PostCreate()
        {
            _argsList[0]["message"] = "Create";
            IPluginRegistrationObjectFactory pluginRegistrationObjectFactory = new PluginRegistrationObjectFactory();
            var assembly = pluginRegistrationObjectFactory.GetAssembly(_fakeReflectionLoader);
            var image = assembly.PluginTypes[0].Steps[0].Images[1];
            Assert.AreEqual("id", image.MessagePropertyName);
        }

        [Test]
        public void MessagePropertyName_Set_To_Target_For_PreCreate()
        {
            _argsList[0]["message"] = "Create";
            _argsList[0]["stage"] = SdkMessageProcessingStep_Stage.Preoperation;
            _argsList[0].Remove("image2Attributes");
            _argsList[0].Remove("image2Type");
            IPluginRegistrationObjectFactory pluginRegistrationObjectFactory = new PluginRegistrationObjectFactory();
            var assembly = pluginRegistrationObjectFactory.GetAssembly(_fakeReflectionLoader);
            var image = assembly.PluginTypes[0].Steps[0].Images[0];
            Assert.AreEqual("Target", image.MessagePropertyName);
        }
        #endregion
    }
}