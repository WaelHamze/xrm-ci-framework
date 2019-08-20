using NUnit.Framework;
using FakeItEasy;
using Xrm.Framework.CI.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets.Test
{
    [TestFixture]
    public class PluginRegistrationHelperTest
    {
        #region Setup
        private IReflectionLoader _fakeLoader;
        private IPluginRegistrationObjectFactory _fakeObjectFactory;

        [SetUp]
        public void SetUp()
        {
            _fakeLoader = A.Fake<IReflectionLoader>();
            _fakeObjectFactory = A.Fake<IPluginRegistrationObjectFactory>();

            A.CallTo(() => _fakeLoader.Initialise(A<string>._, A<string>._));
            A.CallTo(() => _fakeObjectFactory.GetAssembly(A<IReflectionLoader>._)).Returns(new Assembly());
        }

        public void LogVerbose(string text)
        {
            //Used in object creation
        }

        public void LogWarning(string text)
        {
            //Used in object creation
        }
        #endregion

        [Test]
        public void GetPluginRegistrationObject_Makes_Expected_Calls()
        {
            var pluginRegistrationHelper = new PluginRegistrationHelper(LogVerbose, LogWarning, _fakeLoader, _fakeObjectFactory);
            pluginRegistrationHelper.GetPluginRegistrationObject("assemblyPath", "customAttributeClass");
            A.CallTo(() => _fakeLoader.Initialise("assemblyPath", "customAttributeClass")).MustHaveHappened();
            A.CallTo(() => _fakeObjectFactory.GetAssembly(_fakeLoader)).MustHaveHappened();
        }
    }
}
