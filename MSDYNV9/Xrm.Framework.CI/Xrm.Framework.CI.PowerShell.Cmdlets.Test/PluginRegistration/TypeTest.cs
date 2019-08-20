using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.Common;
using C = Xrm.Framework.CI.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets.Test.PluginRegistration
{
    [TestFixture]
    public class TypeTest
    {
        #region SetUp
        private C.Type _typeLeft;
        private C.Type _typeRight;
        private Step _leftStep;
        private Step _rightStep;

        [SetUp]
        public void SetUp()
        {
            _typeLeft = new C.Type { Description = "type left",
                Name = "NameLeft",
                TypeName = "TypeNameLeft",
                WorkflowActivityGroupName = "wf left",
                Id = Guid.NewGuid()
            };
            _typeRight = new C.Type { Description = "type right",
                Name = "NameRight",
                TypeName = "TypeNameRight",
                WorkflowActivityGroupName = "wf right",
                Id = Guid.NewGuid()
            };

            _leftStep = new Step
            {
                Id = Guid.NewGuid(),
                Name = "StepName",
                PrimaryEntityName = "contact",
                Stage = SdkMessageProcessingStep_Stage.Preoperation
            };
            
            _typeLeft.Steps = new List<Step>()
            {
                _leftStep,
                new Step
                {
                    Id = Guid.NewGuid(),
                    Name = "StepName2",
                    PrimaryEntityName = "contact",
                    Stage = SdkMessageProcessingStep_Stage.Postoperation
                }
            };

            _rightStep = new Step
            {
                Id = Guid.NewGuid(),
                Name = "StepName",
                PrimaryEntityName = "contact",
                Stage = SdkMessageProcessingStep_Stage.Preoperation
            };

            _typeRight.Steps = new List<Step>(){ _rightStep };
        }
        #endregion

        #region Tests
        [Test]
        public void Addition_Operator_Imposes_Rhs_Id()
        {
            var addedType = _typeLeft + _typeRight;
            Assert.AreEqual(addedType.Id, _typeRight.Id);
        }

        [Test]
        public void Addition_Operator_Takes_Other_Lhs_Values()
        {
            var addedType = _typeLeft + _typeRight;
            Assert.AreEqual(addedType.Description, _typeLeft.Description);
            Assert.AreEqual(addedType.Name, _typeLeft.Name);
            Assert.AreEqual(addedType.FriendlyName, _typeLeft.FriendlyName);
            Assert.AreEqual(addedType.TypeName, _typeLeft.TypeName);
            Assert.AreEqual(addedType.WorkflowActivityGroupName, _typeLeft.WorkflowActivityGroupName);
        }

        [Test]
        public void Addition_Operator_Keeps_Lhs_Steps_With_No_Rhs_Counterpart()
        {
            var addedType = _typeLeft + _typeRight;
            Assert.AreEqual(2, addedType.Steps.Count);
        }

        [Test]
        public void Addition_Operator_Adds_Steps()
        {
            var addedType = _typeLeft + _typeRight;
            Assert.IsNotNull(addedType.Steps.FirstOrDefault(x => x.Id == _rightStep.Id));
            Assert.IsNull(addedType.Steps.FirstOrDefault(x => x.Id == _leftStep.Id));
        }
        #endregion
    }
}