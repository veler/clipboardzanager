using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using ClipboardZanager.Core.Desktop.Events;
using ClipboardZanager.Core.Desktop.Services;
using ClipboardZanager.Shared.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Core.Desktop.Tests.Service
{
    [TestClass]
    public class MouseAndKeyboardHookServiceTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestUtilities.Initialize();
        }

        [TestMethod]
        public async Task MouseAndKeyboardHookService()
        {
            var service = GetMouseAndKeyboardHookService();
            service.Resume();
            await Task.Delay(500);

            service.RegisterHotKey("CTRL + '", Key.LeftCtrl, Key.OemQuotes);
            service.RegisterHotKey("ALT + V", Key.LeftAlt, Key.V);
            service.HotKeyDetected += Service_HotKeyDetected;
            await Task.Delay(500);

            SendKeys.SendWait("%{v}"); // ALT + V (both key pressed at the same time)

            await Task.Delay(500);
            service.Pause();

            Assert.AreEqual(service.HotKeysEventsHistory.First().Name, "ALT + V");

            service.UnregisterHotKey("ALT + V");
            service.UnregisterHotKey("CTRL + '");
            service.Pause();
        }

        private void Service_HotKeyDetected(object sender, HotKeyEventArgs e)
        {
            e.Handled = true;
        }

        private MouseAndKeyboardHookService GetMouseAndKeyboardHookService()
        {
            return ServiceLocator.GetService<MouseAndKeyboardHookService>();
        }
    }
}
