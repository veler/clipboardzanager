using System.Runtime.InteropServices;

namespace ClipboardZanager.Core.Desktop.Interop.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct BitmapInfoHeader
    {
        internal int biSize;
        internal int biWidth;
        internal int biHeight;
        internal short biPlanes;
        internal short biBitCount;
        internal int biCompression;
        internal int biSizeImage;
        internal int biXPelsPerMeter;
        internal int biYPelsPerMeter;
        internal int biClrUsed;
        internal int biClrImportant;
    }
}
