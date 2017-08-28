using System.Runtime.InteropServices;

namespace ClipboardZanager.Core.Desktop.Interop.Structs
{
    /// <summary>
    /// Represents a point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Point
    {
        internal int X;
        internal int Y;

        public override string ToString()
        {
            return $"{X} {Y}";
        }
    }
}
