using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace ClipboardZanager.Core.Desktop.Interop.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct HotKey
    {
        internal string Name;
        internal List<Key> Keys;
    }
}
