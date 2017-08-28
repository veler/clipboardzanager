using System.Threading;
using System.Threading.Tasks;
using ClipboardZanager.Shared.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Shared.Tests.Core.Worker
{
    [TestClass]
    public class AsyncBackgroundWorkerTests
    {
        private int _progress;
        private bool _cancelled;

        [TestInitialize]
        public void TestInitialize()
        {
            _progress = 0;
            _cancelled = false;
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [TestMethod]
        public async Task AyncBackground()
        {
            var worker = new AsyncBackgroundWorkerMock();

            worker.ProgressChanged += Worker_ProgressChanged;
            worker.WorkEnded += Worker_WorkEnded;
            worker.Start();

            await Task.Delay(300);

            Assert.AreEqual(_progress, 100);
            Assert.IsFalse(_cancelled);
        }

        [TestMethod]
        public async Task AyncBackgroundCancelled()
        {
            var worker = new AsyncBackgroundWorkerMock();

            worker.ProgressChanged += Worker_ProgressChanged;
            worker.WorkEnded += Worker_WorkEnded;
            worker.Start();

            await Task.Delay(100);

            worker.Cancel();

            await Task.Delay(100);

            Assert.AreEqual(_progress, 0);
            Assert.IsTrue(_cancelled);
        }

        private void Worker_WorkEnded(object sender, Events.AsyncBackgroundWorkerEndedEventArgs e)
        {
            _cancelled = e.IsCanceled;
        }

        private void Worker_ProgressChanged(object sender, Events.AsyncBackgroundWorkerProgressEventArgs e)
        {
            _progress = e.Percent;
        }
    }
}
