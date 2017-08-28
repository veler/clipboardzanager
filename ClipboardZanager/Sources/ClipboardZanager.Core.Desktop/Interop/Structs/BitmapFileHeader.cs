using System.Runtime.InteropServices;

namespace ClipboardZanager.Core.Desktop.Interop.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct BitmapFileHeader
    {
        internal static readonly short BM = 0x4d42; // BM

        internal short bfType;
        internal int bfSize;
        internal short bfReserved1;
        internal short bfReserved2;
        internal int bfOffBits;
    }
}
