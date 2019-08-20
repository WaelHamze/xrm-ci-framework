using System;
using NUnit.Framework;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets.Test
{
    [TestFixture]
    public class ReflectionLoaderTest
    {
        [Test]
        public void Loader_Correctly_Initialised()
        {
            //Use own dll. Test class and test custom attribute below
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            IReflectionLoader reflectionLoader = new ReflectionLoader();
            reflectionLoader.Initialise(assembly.Location, "Xrm.Framework.CI.PowerShell.Cmdlets.Test.TestCustomAttribute");

            Assert.AreEqual("Xrm.Framework.CI.PowerShell.Cmdlets.Test.dll", reflectionLoader.AssemblyName);
            Assert.AreEqual(1,reflectionLoader.ClassNames.Count);
            Assert.AreEqual("Xrm.Framework.CI.PowerShell.Cmdlets.Test.TestClass1",reflectionLoader.ClassNames[0]);
            Assert.AreEqual(1,reflectionLoader.Constructors.Count);
            Assert.AreEqual(4, reflectionLoader.Constructors[0].Count);
            Assert.AreEqual(1, (int)reflectionLoader.Constructors[0]["i1"]);
            Assert.AreEqual("a", (string)reflectionLoader.Constructors[0]["s1"]);
            Assert.AreEqual(2, (int)reflectionLoader.Constructors[0]["i2"]);
            Assert.AreEqual("b", (string)reflectionLoader.Constructors[0]["s2"]);

        }
    }

    #region classes for Reflection test
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class TestCustomAttribute : Attribute
    {
        public int Int1;
        public int Int2;
        public string String1;
        public string String2;

        //Added just so there is another unused constructor
        public TestCustomAttribute(int i1, string s1)
        {
            Int1 = i1;
            String1 = s1;
        }

        public TestCustomAttribute(int i1, string s1, int i2, string s2)
        {
            Int1 = i1;
            String1 = s1;
            Int2 = i2;
            String2 = s2;
        }
    }

    [TestCustomAttribute(1,"a",2,"b")]
    public class TestClass1
    {

    }
    #endregion
}
