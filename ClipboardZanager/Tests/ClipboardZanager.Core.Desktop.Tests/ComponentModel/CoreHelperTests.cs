using ClipboardZanager.Core.Desktop.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Core.Desktop.Tests.ComponentModel
{
    [TestClass]
    public class CoreHelperTests
    {
        [TestMethod]
        public void GetApplicationName()
        {
            var hash = CoreHelper.GetApplicationName();

            Assert.AreEqual(hash, "UnitTestApp");
        }
    }
}
