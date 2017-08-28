using System.Runtime.InteropServices;
using ClipboardZanager.Core.Desktop.ComponentModel;

namespace ClipboardZanager.Core.Desktop.Interop.Classes
{
    /// <summary>
    /// The shell link.
    /// </summary>
    [ComImport]
    [Guid(Consts.CShellLink)]
    [ClassInterface(ClassInterfaceType.None)]
    internal class CShellLink
    {
    }
}
