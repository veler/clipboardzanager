using System;
using System.Collections;
using System.Windows.Input;
using ClipboardZanager.Core.Desktop.Enums;

namespace ClipboardZanager.Core.Desktop.ComponentModel
{
    /// <summary>
    /// Provides constants
    /// </summary>
    internal static class Consts
    {
        // Native methods
        internal const string Advapi32 = "Advapi32.dll";
        internal const string Kernel32 = "kernel32.dll";
        internal const string Ole32 = "ole32.dll";
        internal const string PsApi = "psapi.dll";
        internal const string Shcore = "Shcore.dll";
        internal const string Shell32 = "shell32.dll";
        internal const string User32 = "user32.dll";
        internal const string UxTheme = "uxtheme.dll";
        internal const string Wininet = "wininet.dll";

        // Clipboard
        internal static readonly IntPtr HwndMessage = new IntPtr(-3);
        internal const string ClipboardDataFolderName = ".data";
        internal const string DataEntryFileName = ".clipboard";
        internal const string CacheFileName = ".clipboardCache";
        internal const string DropEffectFormatName = "Preferred DropEffect";
        internal const char PasswordMask = '•';
        internal const int ClipboardUpdateMessage = 0x031D;
        internal const int ClipboardDataBufferSize = 2048;
        internal const int OleRetryCount = 10;
        internal const int OleRetryDelay = 100;
        internal const int OleFlushDelay = 10;

        // Keyboard hooking
        internal const string PasteShortcutName = "PasteShortcut";

        // Process & Windows
        internal const string WindowsStoreProcessName = "ApplicationFrameHost";
        internal const int GWL_STYLE = -16;
        internal const int WM_GETICON = 0x7f;
        internal const int WindowsIconsSize = 64;
        internal const int FileIconsSize = 256;

        private const ulong WS_VISIBLE = 0x10000000L;
        private const ulong WS_EX_APPWINDOW = 0x40000;
        private const ulong WS_TILED = 0x00000000L;
        private const ulong WS_TABSTOP = 0x00010000L;
        internal const ulong TARGETWINDOW = WS_VISIBLE | WS_TABSTOP | WS_TILED | WS_EX_APPWINDOW;

        // Interop
        internal const int S_OK = 0x00000000;
        internal const int S_FALSE = 0x00000001;
        internal const int E_INVALIDARG = unchecked((int)0x80070057);
        internal const int E_OUTOFMEMORY = unchecked((int)0x8007000E);
        internal const int E_NOTIMPL = unchecked((int)0x80004001);
        internal const int GA_ROOTOWNER = 3;
        internal const int GMEM_MOVEABLE = 0x0002;
        internal const int GMEM_ZEROINIT = 0x0040;
        internal const int GMEM_DDESHARE = 0x2000;
        internal const int DvETymed = unchecked((int)0x80040069);
        internal const int DvEDvaspect = unchecked((int)0x8004006B);
        internal const int StgEMediumfull = unchecked((int)0x80030070);
        internal const int MONITOR_DEFAULTTONEAREST = 2;
        internal const uint SPI_GETSCREENREADER = 0x0046;

        // COM
        internal const string AppUserModelIdKey = "9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3";
        internal const string PropertyStore = "886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99";
        internal const string ShellLinkW = "000214F9-0000-0000-C000-000000000046";
        internal const string CShellLink = "00021401-0000-0000-C000-000000000046";
        internal const string PersistFile = "0000010b-0000-0000-C000-000000000046";

        // Other
        internal const int SingleInstanceProcessExitCode = 334534;

        internal static readonly string[] WebBrowserIdentifier = { "Microsoft.MicrosoftEdge", "firefox.exe", "chrome.exe", "opera.exe" };
        internal static readonly string WindowsStoreStartupTask = "ClipboardZanagerStartupTask";

        internal static readonly ArrayList DEFAULT_KeyboardHotKeys = new ArrayList { Key.LeftAlt, Key.V };
        internal static readonly ArrayList DEFAULT_DataTypesToKeep = new ArrayList
        {
            SupportedDataType.Text,
            SupportedDataType.Files,
            SupportedDataType.Image,
            SupportedDataType.MicrosoftWord,
            SupportedDataType.MicrosoftExcel,
            SupportedDataType.MicrosoftPowerPoint,
            SupportedDataType.MicrosoftOutlook,
            SupportedDataType.AdobePhotoshop,
        };
    }
}
