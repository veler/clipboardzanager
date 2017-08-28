using System.Collections.Generic;
using System.Linq;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Core.Desktop.Tests.ComponentModel
{

    [TestClass]
    public class DataHelperTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestUtilities.Initialize();
        }

        [TestMethod]
        public void StringsToByte()
        {
            var entry = "Hello";

            var bytes = DataHelper.ToByteArray(entry);
            var result = DataHelper.FromByteArray<string>(bytes);

            Assert.IsTrue(result.SequenceEqual(entry));
        }

        [TestMethod]
        public void ObjectToBase64()
        {
            var entry = new List<string> { "Hello", "World" };

            var bytes = DataHelper.ToBase64(entry);
            var result = DataHelper.FromBase64<List<string>>(bytes);

            Assert.IsTrue(result.SequenceEqual(entry));
        }

        [TestMethod]
        public void BitmapToBitmapImage()
        {
            var bitmap = BitmapHelper.GenerateBitmap();

            var bitmapImage = DataHelper.BitmapToBitmapImage(bitmap, null);

            var stride = bitmapImage.PixelWidth * 4;
            var size = bitmapImage.PixelHeight * stride;
            var pixels2 = new byte[size];
            bitmapImage.CopyPixels(pixels2, stride, 0);

            var red = pixels2[0];
            var green = pixels2[1];
            var blue = pixels2[2];
            var alpha = pixels2[3];

            Assert.AreEqual(red, 255);
            Assert.AreEqual(green, 255);
            Assert.AreEqual(blue, 255);
            Assert.AreEqual(alpha, 255);
        }

        [TestMethod]
        public void BitmapToBytes()
        {
            var bitmapImage = BitmapHelper.GenerateBitmapImage();

            var bytes = DataHelper.BitmapSourceToByteArray(bitmapImage);
            var result = DataHelper.ByteArrayToBitmapSource(bytes);

            var stride = result.PixelWidth * 4;
            var size = result.PixelHeight * stride;
            var pixels2 = new byte[size];
            result.CopyPixels(pixels2, stride, 0);

            var red = pixels2[0];
            var green = pixels2[1];
            var blue = pixels2[2];
            var alpha = pixels2[3];

            Assert.AreEqual(red, 255);
            Assert.AreEqual(green, 255);
            Assert.AreEqual(blue, 255);
            Assert.AreEqual(alpha, 255);
        }
    }
}
