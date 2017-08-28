using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Hooking;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Core.Desktop.Tests.Hooking
{
    [TestClass]
    public class KeyboardHookTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestUtilities.Initialize();
        }

        [TestMethod]
        public void KeyboardHook()
        {
            using (var hooking = new KeyboardHook())
            {
                hooking.HandleKeyboardKeyRelease = false;

                Task.Delay(100).Wait();
                SendKeys.SendWait("%{v}"); // ALT + V (both key pressed at the same time)
                Task.Delay(100).Wait();

                Assert.AreEqual(hooking.EventsHistory.Count, 2);
                Assert.IsTrue(hooking.EventsHistory[0].Key == Key.V && hooking.EventsHistory[0].State == KeyState.Pressed);
                Assert.IsTrue(hooking.EventsHistory[1].Key == Key.LeftAlt && hooking.EventsHistory[1].State == KeyState.Pressed);

                hooking.Pause();

                Task.Delay(100).Wait();
                SendKeys.SendWait("%{v}"); // ALT + V (both key pressed at the same time)
                Task.Delay(100).Wait();

                hooking.Resume();

                Task.Delay(100).Wait();
                SendKeys.SendWait("%{v}"); // ALT + V (both key pressed at the same time)
                Task.Delay(100).Wait();

                Assert.AreEqual(hooking.EventsHistory.Count, 4);
                Assert.IsTrue(hooking.EventsHistory[2].Key == Key.V && hooking.EventsHistory[2].State == KeyState.Pressed);
                Assert.IsTrue(hooking.EventsHistory[3].Key == Key.LeftAlt && hooking.EventsHistory[3].State == KeyState.Pressed);
            }
        }
    }
}
