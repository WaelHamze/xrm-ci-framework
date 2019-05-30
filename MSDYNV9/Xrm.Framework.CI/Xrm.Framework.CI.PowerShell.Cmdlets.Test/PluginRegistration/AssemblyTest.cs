using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.PowerShell.Cmdlets.Test.PluginRegistration
{
    [TestFixture]
    public class AssemblyTest
    {
        #region SetUp
        private Assembly _assemblyLeft;
        private Assembly _assemblyRight;
        private Type _typeLeft;
        private Type _typeRight;

        [SetUp]
        public void SetUp()
        {
            _assemblyLeft = new Assembly
            {
                Id = Guid.NewGuid(),
                IsolationMode = PluginAssembly_IsolationMode.Sandbox,
                Name = "lhs",
                SourceType = PluginAssembly_SourceType.Database
            };

            _assemblyRight = new Assembly
            {
                Id = Guid.NewGuid(),
                IsolationMode = PluginAssembly_IsolationMode.None,
                Name = "rhs",
                SourceType = PluginAssembly_SourceType.Disk
            };

            _typeLeft = new Type {Description = "type left",Name = "Name", TypeName = "TypeName", Id =Guid.NewGuid()};
            _assemblyLeft.PluginTypes = new List<Type>
            {
                _typeLeft, new Type {Description = "type left 2", Id = Guid.NewGuid()}
            };

            _typeRight = new Type { Description = "type right", Name = "Name", TypeName = "TypeName",  Id = Guid.NewGuid() };
            _assemblyRight.PluginTypes = new List<Type>() { _typeRight };
        }
        #endregion

        #region Tests
        [Test]
        public void Addition_Operator_Imposes_Rhs_Id()
        {
            var addedAssembly = _assemblyLeft + _assemblyRight;
            Assert.AreEqual(addedAssembly.Id, _assemblyRight.Id);
        }

        [Test]
        public void Addition_Operator_Takes_Other_Lhs_Values()
        {
            var addedAssembly = _assemblyLeft + _assemblyRight;
            Assert.AreEqual(addedAssembly.IsolationMode, _assemblyLeft.IsolationMode);
            Assert.AreEqual(addedAssembly.Name, _assemblyLeft.Name);
            Assert.AreEqual(addedAssembly.SourceType, _assemblyLeft.SourceType);
        }

        [Test]
        public void Addition_Operator_Keeps_Lhs_Types_With_No_Rhs_Counterpart()
        {
            var addedAssembly = _assemblyLeft + _assemblyRight;
            Assert.AreEqual(2, addedAssembly.PluginTypes.Count);
        }

        [Test]
        public void Addition_Operator_Adds_Types()
        {
            var addedAssembly = _assemblyLeft + _assemblyRight;
            Assert.IsNotNull(addedAssembly.PluginTypes.FirstOrDefault(x => x.Id == _typeRight.Id));
            Assert.IsNull(addedAssembly.PluginTypes.FirstOrDefault(x => x.Id == _typeLeft.Id));
        }
        #endregion
    }
}
