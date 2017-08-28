using System.Runtime.InteropServices;

namespace ClipboardZanager.Core.Desktop.Interop.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        public Rect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        internal int Left;
        internal int Top;
        internal int Right;
        internal int Bottom;
    }
}
