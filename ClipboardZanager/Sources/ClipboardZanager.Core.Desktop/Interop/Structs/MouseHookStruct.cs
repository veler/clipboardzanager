using System;
using System.Runtime.InteropServices;

namespace ClipboardZanager.Core.Desktop.Interop.Structs
{
    /// <summary>
    /// Contains information about a mouse event passed to a WH_MOUSE hook procedure, MouseProc. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct MouseHookStruct
    {
        internal Point pt;
        internal uint mouseData;
        internal uint flags;
        internal uint time;
        internal IntPtr dwExtraInfo;
    }
}
