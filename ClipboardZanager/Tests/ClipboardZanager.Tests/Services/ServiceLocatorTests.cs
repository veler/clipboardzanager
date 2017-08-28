using ClipboardZanager.Shared.Exceptions;
using ClipboardZanager.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Tests.Services
{
    [TestClass]
    public class ServiceLocatorTests
    {
        [TestMethod]
        public void ServiceLocator()
        {
            Shared.Services.ServiceLocator.SettingProvider = new ServiceSettingProviderMock();
            var service1 = Shared.Services.ServiceLocator.GetService<ServiceMock>();
            var service2 = Shared.Services.ServiceLocator.GetService<ServiceMock>();

            Assert.AreSame(service1, service2);
            Assert.IsFalse(service1.Reseted);

            Shared.Services.ServiceLocator.ResetAll();

            Assert.IsTrue(service1.Reseted);
        }

        [TestMethod]
        [ExpectedException(typeof(NotNullRequiredException))]
        public void ServiceLocatorNoSettingProvider()
        {
            Shared.Services.ServiceLocator.SettingProvider = null;
            var service1 = Shared.Services.ServiceLocator.GetService<ServiceMock>();
        }
    }
}
