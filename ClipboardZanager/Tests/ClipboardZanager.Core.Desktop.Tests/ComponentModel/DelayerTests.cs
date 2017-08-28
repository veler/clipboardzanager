using System;
using System.Threading.Tasks;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Core.Desktop.Tests.ComponentModel
{
    [TestClass]
    public class DelayerTests
    {
        [TestMethod]
        public void Delayer()
        {
            var actionCalled = false;

            DispatcherUtil.ExecuteOnDispatcherThread(() =>
            {
                var delayer = new Delayer<string>(TimeSpan.FromMilliseconds(100));
                delayer.Action += (sender, args) =>
                {
                    if (args.Data == "hello")
                    {
                        actionCalled = true;
                    }
                };

                Assert.IsFalse(actionCalled);
                delayer.ResetAndTick("hello");

                Task.Delay(100).Wait();
                DispatcherUtil.DoEvents();
                Assert.IsFalse(actionCalled);

            }, 1);

            Task.Delay(100).Wait();
            DispatcherUtil.DoEvents();
            Assert.IsTrue(actionCalled);
        }
    }
}
