using System.Runtime.InteropServices;

namespace ClipboardZanager.Core.Desktop.Interop.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Gesture
    {
        internal string Name;
        internal int[,] CheckPoinstArray;
    }
}
