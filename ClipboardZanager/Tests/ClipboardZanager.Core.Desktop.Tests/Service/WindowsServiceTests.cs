using System.Linq;
using System.Threading.Tasks;
using ClipboardZanager.Core.Desktop.Services;
using ClipboardZanager.Shared.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Core.Desktop.Tests.Service
{
    [TestClass]
    public class WindowsServiceTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestUtilities.Initialize();
        }

        [TestMethod]
        public void WindowsService_RefreshWindowsList()
        {
            var service = GetWindowsService();
            service.RefreshWindows();

            Task.Delay(300).Wait();

            Assert.IsTrue(service.WindowsList.Any(w => !w.IsWindowsStoreApp && w.Title.Contains("Microsoft Visual Studio") && w.Icon != null && w.ApplicationIdentifier == @"C:\Windows\explorer.exe" && w.Process.ProcessName == "devenv"));
        }

        [TestMethod]
        public void WindowsService_ForegroundWindow()
        {
            var service = GetWindowsService();
            var window = service.GetForegroundWindow();

            Assert.IsTrue(window.Title.Contains("Microsoft Visual Studio"));
            Assert.IsFalse(window.IsWindowsStoreApp);
            Assert.IsNotNull(window.Icon);
            Assert.AreEqual(window.ApplicationIdentifier, @"C:\Windows\explorer.exe");
            Assert.AreEqual(window.Process.ProcessName, "devenv");
        }

        private WindowsService GetWindowsService()
        {
            return ServiceLocator.GetService<WindowsService>();
        }
    }
}
