using System;
using System.Runtime.InteropServices;
using ClipboardZanager.Core.Desktop.Enums;

namespace ClipboardZanager.Core.Desktop.Interop.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }
}
