using System.IO;
using System.Linq;
using ClipboardZanager.Core.Desktop.Clipboard;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Core.Desktop.Tests.Clipboard
{
    [TestClass]
    public class ClipboardTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestUtilities.Initialize();
        }

        [TestMethod]
        public void ClipboardWriterAndReader()
        {
            using (var clipboardWriter = new ClipboardWriter())
            {
                var value = new MemoryStream(new byte[] { 72, 101, 108, 108, 111 }); // "Hello" in binary

                clipboardWriter.AddData("ASCIITextTest", value);

                clipboardWriter.Flush();
            }

            var dataFromClipboard = (MemoryStream)System.Windows.Clipboard.GetData("ASCIITextTest");
            Assert.IsTrue(dataFromClipboard.ToArray().SequenceEqual(new byte[] { 72, 101, 108, 108, 111 }));

            var dataFromClipboard2 = new MemoryStream();
            var dataObject = (System.Windows.DataObject)System.Windows.Clipboard.GetDataObject();

            using (var clipboardReader = new ClipboardReader(dataObject))
            {
                Assert.IsFalse(clipboardReader.IsReadable);
                Assert.IsFalse(clipboardReader.CanReadNextBlock());

                foreach (var format in dataObject.GetFormats())
                {
                    clipboardReader.BeginRead(format);
                    Assert.IsTrue(clipboardReader.IsReadable);
                    Assert.IsTrue(clipboardReader.CanReadNextBlock());

                    while (clipboardReader.CanReadNextBlock())
                    {
                        var buffer = clipboardReader.ReadNextBlock();
                        dataFromClipboard2.Write(buffer, 0, buffer.Length);
                    }

                    clipboardReader.EndRead();
                }
            }

            Assert.IsTrue(dataFromClipboard2.ToArray().SequenceEqual(new byte[] { 72, 101, 108, 108, 111 }));
        }
    }
}
