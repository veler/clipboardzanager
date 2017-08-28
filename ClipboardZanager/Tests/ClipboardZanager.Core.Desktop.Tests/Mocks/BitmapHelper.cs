using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace ClipboardZanager.Core.Desktop.Tests.Mocks
{
    internal static class BitmapHelper
    {
        internal static Bitmap GenerateBitmap()
        {
            var bitmap = new Bitmap(1, 1);
            bitmap.SetPixel(0, 0, Color.White);
            return bitmap;
        }

        internal static BitmapImage GenerateBitmapImage()
        {
            const int height = 1;
            const int width = 1;
            var writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            var pixels = new byte[height, width, 4];
            var rect = new Int32Rect(0, 0, width, height);
            var stride = 4 * width;
            var pixels1D = new byte[height * width * 4];
            var index = 0;

            pixels[0, 0, 0] = 255;
            pixels[0, 0, 1] = 255;
            pixels[0, 0, 2] = 255;
            pixels[0, 0, 3] = 255;

            for (var row = 0; row < height; row++)
            {
                for (var col = 0; col < width; col++)
                {
                    for (var i = 0; i < 4; i++)
                        pixels1D[index++] = pixels[row, col, i];
                }
            }
            writeableBitmap.WritePixels(rect, pixels1D, stride, 0);

            var bitmapImage = new BitmapImage();
            using (var memoryStream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
                encoder.Save(memoryStream);
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }
    }
}
