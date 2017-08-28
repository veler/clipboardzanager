using System.IO;
using System.Linq;
using System.Text;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Core.Desktop.Tests.IO
{
    [TestClass]
    public class AesStreamTests
    {
        [TestMethod]
        public void AesStreamWrite()
        {
            var data = Encoding.ASCII.GetBytes("Hello");
            Assert.IsTrue(data.ToArray().SequenceEqual(new byte[] { 72, 101, 108, 108, 111 }));

            var password = SecurityHelper.ToSecureString("MyPassword");
            using (var baseStream = new MemoryStream())
            using (var aesStream = new AesStream(baseStream, password, SecurityHelper.GetSaltKeys(password).GetBytes(16)))
            {
                aesStream.Write(data, 0, data.Length);
                aesStream.Position = 0;

                var cryptedData = new byte[baseStream.Length];
                baseStream.Read(cryptedData, 0, cryptedData.Length);
                Assert.IsTrue(cryptedData.ToArray().SequenceEqual(new byte[] { 238, 75, 117, 248, 55 }));
            }
        }

        [TestMethod]
        public void AesStreamRead()
        {
            var cryptedData = new byte[] { 238, 75, 117, 248, 55 };
            var password = SecurityHelper.ToSecureString("MyPassword");

            using (var baseStream = new MemoryStream(cryptedData))
            using (var aesStream = new AesStream(baseStream, password, SecurityHelper.GetSaltKeys(password).GetBytes(16)))
            {
                var data = new byte[aesStream.Length];
                aesStream.Read(data, 0, data.Length);

                Assert.IsTrue(data.ToArray().SequenceEqual(new byte[] { 72, 101, 108, 108, 111 }));
                Assert.AreEqual(Encoding.ASCII.GetString(data), "Hello");
            }
        }
    }
}
