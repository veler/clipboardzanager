using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Text;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Interop.Classes;
using ClipboardZanager.Core.Desktop.Interop.Interfaces;
using ClipboardZanager.Core.Desktop.Interop.Structs;
using static ClipboardZanager.Core.Desktop.ComponentModel.Delegates;

namespace ClipboardZanager.Core.Desktop.Interop
{
    /// <summary>
    /// Provides a set of native methods
    /// </summary>
    internal static class NativeMethods
    {
        #region Advapi32

        /// <summary>
        /// Notifies the caller about changes to the attributes or contents of a specified registry key.
        /// </summary>
        /// <param name="hKey">A handle to an open registry key. This handle is returned by the RegCreateKeyEx or RegOpenKeyEx function.</param>
        /// <param name="watchSubtree">If this parameter is TRUE, the function reports changes in the specified key and its subkeys. If the parameter is FALSE, the function reports changes only in the specified key.</param>
        /// <param name="notifyFilter">A value that indicates the changes that should be reported. This parameter can be one or more of the following values.</param>
        /// <param name="hEvent">A handle to an event. If the fAsynchronous parameter is TRUE, the function returns immediately and changes are reported by signaling this event. If fAsynchronous is FALSE, hEvent is ignored.</param>
        /// <param name="asynchronous">If this parameter is TRUE, the function returns immediately and reports changes by signaling the specified event. If this parameter is FALSE, the function does not return until a change has occurred. </param>
        /// <returns>If the function succeeds, the return value is ERROR_SUCCESS.</returns>
        [DllImport(Consts.Advapi32)]
        internal static extern int RegNotifyChangeKeyValue(IntPtr hKey, bool watchSubtree, RegistryNotifyChange notifyFilter, IntPtr hEvent, bool asynchronous);

        #endregion

        #region Kernel32

        /// <summary>
        /// Closes an open object handle.
        /// </summary>
        /// <param name="hHandle">A valid handle to an open object.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport(Consts.Kernel32, SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hHandle);

        /// <summary>
        /// Retrieves a module handle for the specified module. The module must have been loaded by the calling process.
        /// </summary>
        /// <param name="lpModuleName">The name of the loaded module (either a .dll or .exe file).</param>
        /// <returns>If the function succeeds, the return value is a handle to the specified module. If the function fails, the return value is NULL.</returns>
        [DllImport(Consts.Kernel32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// Allocates the specified number of bytes from the heap.
        /// </summary>
        /// <param name="uFlags">The memory allocation attributes. If zero is specified, the default is GMEM_FIXED.</param>
        /// <param name="dwBytes">The number of bytes to allocate. If this parameter is zero and the uFlags parameter specifies GMEM_MOVEABLE, the function returns a handle to a memory object that is marked as discarded.</param>
        /// <returns>If the function succeeds, the return value is a handle to the newly allocated memory object.</returns>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Consts.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GlobalAlloc(int uFlags, IntPtr dwBytes);

        /// <summary>
        /// Changes the size or attributes of a specified global memory object. The size can increase or decrease.
        /// </summary>
        /// <param name="handle">A handle to the global memory object to be reallocated. This handle is returned by either the GlobalAlloc or GlobalReAlloc function.</param>
        /// <param name="bytes">The new size of the memory block, in bytes. If uFlags specifies GMEM_MODIFY, this parameter is ignored.</param>
        /// <param name="flags">The reallocation options. If GMEM_MODIFY is specified, the function modifies the attributes of the memory object only (the dwBytes parameter is ignored.) Otherwise, the function reallocates the memory object.</param>
        /// <returns>If the function succeeds, the return value is a handle to the reallocated memory object.</returns>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Consts.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GlobalReAlloc(HandleRef handle, IntPtr bytes, int flags);

        /// <summary>
        /// Locks a global memory object and returns a pointer to the first byte of the object's memory block.
        /// </summary>
        /// <param name="handle">A handle to the global memory object. This handle is returned by either the GlobalAlloc or GlobalReAlloc function.</param>
        /// <returns>If the function succeeds, the return value is a pointer to the first byte of the memory block.</returns>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Consts.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GlobalLock(HandleRef handle);

        /// <summary>
        /// Decrements the lock count associated with a memory object that was allocated with GMEM_MOVEABLE. This function has no effect on memory objects allocated with GMEM_FIXED.
        /// </summary>
        /// <param name="handle">A handle to the global memory object. This handle is returned by either the GlobalAlloc or GlobalReAlloc function.</param>
        /// <returns>If the memory object is still locked after decrementing the lock count, the return value is a nonzero value. If the memory object is unlocked after decrementing the lock count, the function returns zero and GetLastError returns NO_ERROR.</returns>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Consts.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool GlobalUnlock(HandleRef handle);

        /// <summary>
        /// Frees the specified global memory object and invalidates its handle.
        /// </summary>
        /// <param name="handle">A handle to the global memory object. This handle is returned by either the GlobalAlloc or GlobalReAlloc function. It is not safe to free memory allocated with LocalAlloc.</param>
        /// <returns>If the function succeeds, the return value is NULL.</returns>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Consts.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GlobalFree(HandleRef handle);

        /// <summary>
        /// Retrieves the current size of the specified global memory object, in bytes.
        /// </summary>
        /// <param name="handle">A handle to the global memory object. This handle is returned by either the GlobalAlloc or GlobalReAlloc function.</param>
        /// <returns>If the function succeeds, the return value is the size of the specified global memory object, in bytes.</returns>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Consts.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GlobalSize(HandleRef handle);

        /// <summary>
        /// Opens an existing local process object.
        /// </summary>
        /// <param name="dwDesiredAccess">The access to the process object. This access right is checked against the security descriptor for the process. This parameter can be one or more of the process access rights.</param>
        /// <param name="bInheritHandle">If this value is TRUE, processes created by this process will inherit the handle. Otherwise, the processes do not inherit this handle.</param>
        /// <param name="dwProcessId">The identifier of the local process to be opened.</param>
        /// <returns>If the function succeeds, the return value is an open handle to the specified process.</returns>
        [DllImport(Consts.Kernel32, SetLastError = true)]
        internal static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        /// <summary>
        /// Retrieves the full name of the executable image for the specified process.
        /// </summary>
        /// <param name="hprocess">A handle to the process. This handle must be created with the PROCESS_QUERY_INFORMATION or PROCESS_QUERY_LIMITED_INFORMATION access right. For more information, see Process Security and Access Rights.</param>
        /// <param name="dwFlags">This parameter can be one of the following values.</param>
        /// <param name="lpExeName">The path to the executable image. If the function succeeds, this string is null-terminated.</param>
        /// <param name="size">On input, specifies the size of the lpExeName buffer, in characters. On success, receives the number of characters written to the buffer, not including the null-terminating character.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport(Consts.Kernel32, SetLastError = true)]
        internal static extern bool QueryFullProcessImageName(IntPtr hprocess, int dwFlags, StringBuilder lpExeName, out int size);

        /// <summary>
        /// Registers the active instance of an application for restart.
        /// </summary>
        /// <param name="commandLineArgs">A pointer to a Unicode string that specifies the command-line arguments for the application when it is restarted. The maximum size of the command line that you can specify is RESTART_MAX_CMD_LINE characters. Do not include the name of the executable in the command line; this function adds it for you. If this parameter is NULL or an empty string, the previously registered command line is removed.If the argument contains spaces, use quotes around the argument.</param>
        /// <param name="Flags"></param>
        /// <returns>This function returns S_OK on success or one of the following error codes.</returns>
        [DllImport(Consts.Kernel32, SetLastError = true)]
        internal static extern int RegisterApplicationRestart([MarshalAs(UnmanagedType.LPWStr)] string commandLineArgs, int Flags);

        #endregion

        #region Ole32

        /// <summary>
        /// The PropVariantClear function frees all elements that can be freed in a given <see cref="PropVariant"/> structure. For complex elements with known element pointers, the underlying elements are freed prior to freeing the containing element.
        /// </summary>
        /// <param name="pvar">A pointer to an initialized <see cref="PropVariant"/> structure for which any deallocatable elements are to be freed. On return, all zeroes are written to the <see cref="PropVariant"/> structure.</param>
        [DllImport(Consts.Ole32, SetLastError = true, PreserveSig = false)]
        internal static extern void PropVariantClear([In, Out] PropVariant pvar);

        /// <summary>
        /// Places a pointer to a specific data object onto the clipboard. This makes the data object accessible to the OleGetClipboard function.
        /// </summary>
        /// <param name="pDataObj">Pointer to the IDataObject interface on the data object from which the data to be placed on the clipboard can be obtained. This parameter can be NULL; in which case the clipboard is emptied.</param>
        /// <returns>This function returns <see cref="Consts.S_OK"/> on success.</returns>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Consts.Ole32, ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern int OleSetClipboard(IDataObject pDataObj);

        /// <summary>
        /// Carries out the clipboard shutdown sequence. It also releases the IDataObject pointer that was placed on the clipboard by the OleSetClipboard function.
        /// </summary>
        /// <returns>This function returns <see cref="Consts.S_OK"/> on success.</returns>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Consts.Ole32, ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern int OleFlushClipboard();

        /// <summary>
        /// Closes the COM library on the apartment, releases any class factories, other COM objects, or servers held by the apartment, disables RPC on the apartment, and frees any resources the apartment maintains. 
        /// </summary>
        /// <returns>This function returns <see cref="Consts.S_OK"/> on success.</returns>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(Consts.Ole32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int OleUninitialize();

        /// <summary>
        /// Initializes the COM library on the current apartment, identifies the concurrency model as single-thread apartment (STA), and enables additional functionality described in the Remarks section below. Applications must initialize the COM library before they can call COM library functions other than CoGetMalloc and memory allocation functions.
        /// </summary>
        /// <param name="val">This parameter is reserved and must be NULL.</param>
        /// <returns>This function returns <see cref="Consts.S_OK"/> on success.</returns>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Consts.Ole32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int OleInitialize(IntPtr val);

        /// <summary>
        /// Frees the specified storage medium.
        /// </summary>
        /// <param name="medium">Pointer to the storage medium that is to be freed.</param>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Consts.Ole32, ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern void ReleaseStgMedium(ref STGMEDIUM medium);

        #endregion

        #region PsApi

        /// <summary>
        /// Removes as many pages as possible from the working set of the specified process.
        /// </summary>
        /// <param name="hwProc">A handle to the process. The handle must have the PROCESS_QUERY_INFORMATION or PROCESS_QUERY_LIMITED_INFORMATION access right and the PROCESS_SET_QUOTA access right.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport(Consts.PsApi, SetLastError = true)]
        internal static extern int EmptyWorkingSet(IntPtr hwProc);

        #endregion

        #region Shcore

        /// <summary>
        /// Gets the scale factor of a specific monitor. This function replaces GetScaleFactorForDevice.
        /// </summary>
        /// <param name="hmonitor">The monitor's handle.</param>
        /// <param name="deviceScaleFactor">When this function returns successfully, this value points to one of the DEVICE_SCALE_FACTOR values that specify the scale factor of the specified monitor. </param>
        /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [DllImport(Consts.Shcore, SetLastError = true)]
        internal static extern IntPtr GetScaleFactorForMonitor([In]IntPtr hmonitor, [Out]out int deviceScaleFactor);

        #endregion

        #region Shell32

        /// <summary>
        /// Retrieves an object that represents a specific window's collection of properties, which allows those properties to be queried or set.
        /// </summary>
        /// <param name="handle">A handle to the window whose properties are being retrieved.</param>
        /// <param name="riid">A reference to the IID of the property store object to retrieve through ppv. This is typically IID_IPropertyStore.</param>
        /// <param name="propertyStore">When this function returns, contains the interface pointer requested in riid. This is typically IPropertyStore.</param>
        /// <returns>If this function succeeds, it returns <see cref="Consts.S_OK"/>. Otherwise, it returns an HRESULT error code.</returns>
        [DllImport(Consts.Shell32, SetLastError = true)]
        internal static extern int SHGetPropertyStoreForWindow(IntPtr handle, ref Guid riid, out IPropertyStore propertyStore);

        /// <summary>
        /// Retrieves a handle to an indexed icon found in a file or an icon found in an associated executable file.
        /// </summary>
        /// <param name="hInst">A handle to the instance of the application calling the function. </param>
        /// <param name="lpIconPath">The full path and file name of the file that contains the icon. The function extracts the icon handle from that file, or from an executable file associated with that file. If the icon handle is obtained from an executable file, the function stores the full path and file name of that executable in the string pointed to by lpIconPath. </param>
        /// <param name="lpiIcon">The index of the icon whose handle is to be obtained. If the icon handle is obtained from an executable file, the function stores the icon's identifier in this parameter. </param>
        /// <returns>If the function succeeds, the return value is an icon handle. If the icon is extracted from an associated executable file, the function stores the full path and file name of the executable file in the string pointed to by lpIconPath, and stores the icon's identifier in the variable pointed to by lpiIcon.</returns>
        [DllImport(Consts.Shell32)]
        internal static extern IntPtr ExtractAssociatedIcon(IntPtr hInst, StringBuilder lpIconPath, out ushort lpiIcon);

        #endregion

        #region User32

        /// <summary>
        /// Places the given window in the system-maintained clipboard format listener list.
        /// </summary>
        /// <param name="hwnd">A handle to the window to be placed in the clipboard format listener list.</param>
        /// <returns>Returns TRUE if successful, FALSE otherwise.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AddClipboardFormatListener(IntPtr hwnd);

        /// <summary>
        /// Passes the hook information to the next hook procedure in the current hook chain. A hook procedure can call this function either before or after processing the hook information.
        /// </summary>
        /// <param name="hhk">This parameter is ignored.</param>
        /// <param name="nCode">The hook code passed to the current hook procedure. The next hook procedure uses this code to determine how to process the hook information.</param>
        /// <param name="wParam">The wParam value passed to the current hook procedure. The meaning of this parameter depends on the type of hook associated with the current hook chain.</param>
        /// <param name="lParam">The lParam value passed to the current hook procedure. The meaning of this parameter depends on the type of hook associated with the current hook chain.</param>
        /// <returns>This value is returned by the next hook procedure in the chain. The current hook procedure must also return this value. The meaning of the return value depends on the hook type. For more information, see the descriptions of the individual hook procedures.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Enumerates all top-level windows on the screen by passing the handle to each window, in turn, to an application-defined callback function. EnumWindows continues until the last top-level window is enumerated or the callback function returns FALSE.
        /// </summary>
        /// <param name="lpEnumFunc">A pointer to an application-defined callback function.</param>
        /// <param name="lParam">An application-defined value to be passed to the callback function. </param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        internal static extern int EnumWindows(EnumWindowsCallback lpEnumFunc, int lParam);

        /// <summary>
        /// Retrieves the handle to the ancestor of the specified window.
        /// </summary>
        /// <param name="hwnd">A handle to the window whose ancestor is to be retrieved. If this parameter is the desktop window, the function returns NULL.</param>
        /// <param name="gaFlags">The ancestor to be retrieved.</param>
        /// <returns>The return value is the handle to the ancestor window.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        internal static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags = Consts.GA_ROOTOWNER);

        /// <summary>
        /// Retrieves the specified 32-bit (DWORD) value from the WNDCLASSEX structure associated with the specified window.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs. </param>
        /// <param name="nIndex">The value to be retrieved. To retrieve a value from the extra class memory, specify the positive, zero-based byte offset of the value to be retrieved. Valid values are in the range zero through the number of bytes of extra class memory, minus four; for example, if you specified 12 or more bytes of extra class memory, a value of 8 would be an index to the third integer.</param>
        /// <returns>If the function succeeds, the return value is the requested value.</returns>
        [DllImport(Consts.User32, SetLastError = true, EntryPoint = "GetClassLong")]
        internal static extern int GetClassLongPtr32(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Retrieves the specified value from the WNDCLASSEX structure associated with the specified window.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The value to be retrieved. To retrieve a value from the extra class memory, specify the positive, zero-based byte offset of the value to be retrieved. Valid values are in the range zero through the number of bytes of extra class memory, minus eight; for example, if you specified 24 or more bytes of extra class memory, a value of 16 would be an index to the third integer.</param>
        /// <returns>If the function succeeds, the return value is the requested value.</returns>
        [DllImport(Consts.User32, SetLastError = true, EntryPoint = "GetClassLongPtr")]
        internal static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Moves the cursor to the specified screen coordinates. If the new coordinates are not within the screen rectangle set by the most recent ClipCursor function call, the system automatically adjusts the coordinates so that the cursor stays within the rectangle.
        /// </summary>
        /// <param name="pt">The <see cref="Point"/> that corresponds to the cursor's coordonates.</param>
        /// <returns>Returns nonzero if successful or zero otherwise.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Point pt);

        /// <summary>
        /// Retrieves a handle to the foreground window (the window with which the user is currently working). The system assigns a slightly higher priority to the thread that creates the foreground window than it does to other threads.
        /// </summary>
        /// <returns>The return value is a handle to the foreground window. The foreground window can be NULL in certain circumstances, such as when a window is losing activation.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        internal static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Brings the thread that created the specified window into the foreground and activates the window. Keyboard input is
        /// directed to the window, and various visual cues are changed for the user. The system assigns a slightly higher
        /// priority to the thread that created the foreground window than it does to other threads.
        /// <para>See for https://msdn.microsoft.com/en-us/library/windows/desktop/ms633539%28v=vs.85%29.aspx more information.</para>
        /// </summary>
        /// <returns><c>true</c> or nonzero if the window was brought to the foreground, <c>false</c> or zero If the window was not brought to the foreground.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Retrieves the status of the specified virtual key. The status specifies whether the key is up, down, or toggled (on, off—alternating each time the key is pressed). 
        /// </summary>
        /// <param name="keyCode">A virtual key. If the desired virtual key is a letter or digit (A through Z, a through z, or 0 through 9), nVirtKey must be set to the ASCII value of that character. For other keys, it must be a virtual-key code.</param>
        /// <returns>The return value specifies the status of the specified virtual key, as follows: If the high-order bit is 1, the key is down; otherwise, it is up. If the low-order bit is 1, the key is toggled.A key, such as the CAPS LOCK key, is toggled if it is turned on.The key is off and untoggled if the low-order bit is 0. A toggle key's indicator light (if any) on the keyboard will be on when the key is toggled, and off when the key is untoggled.</returns>
        [DllImport(Consts.User32, SetLastError = true, CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        internal static extern short GetKeyState(int keyCode);

        /// <summary>
        /// Determines which pop-up window owned by the specified window was most recently active. 
        /// </summary>
        /// <param name="hWnd">A handle to the owner window.</param>
        /// <returns>The return value identifies the most recently active pop-up window.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        internal static extern IntPtr GetLastActivePopup(IntPtr hWnd);

        /// <summary>
        /// Retrieves a handle to the Shell's desktop window.
        /// </summary>
        /// <returns>The return value is the handle of the Shell's desktop window. If no Shell process is present, the return value is NULL.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        internal static extern IntPtr GetShellWindow();

        /// <summary>
        /// Retrieves information about the specified window. The function also retrieves the 32-bit (DWORD) value at the specified offset into the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The zero-based offset to the value to be retrieved. Valid values are in the range zero through the number of bytes of extra window memory, minus four; for example, if you specified 12 or more bytes of extra memory, a value of 8 would be an index to the third 32-bit integer.</param>
        /// <returns>If the function succeeds, the return value is the requested value.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        internal static extern ulong GetWindowLongA(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Copies the text of the specified window's title bar (if it has one) into a buffer. If the specified window is a control, the text of the control is copied. However, GetWindowText cannot retrieve the text of a control in another application.
        /// </summary>
        /// <param name="hWnd">A handle to the window or control containing the text.</param>
        /// <param name="lpString">The buffer that will receive the text. If the string is as long or longer than the buffer, the string is truncated and terminated with a null character.</param>
        /// <param name="nMaxCount">The maximum number of characters to copy to the buffer, including the null character. If the text exceeds this limit, it is truncated.</param>
        [DllImport(Consts.User32, SetLastError = true)]
        internal static extern void GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the process that created the window.
        /// </summary>
        /// <param name="hwnd">A handle to the window.</param>
        /// <param name="processId">A pointer to a variable that receives the process identifier. If this parameter is not NULL, GetWindowThreadProcessId copies the identifier of the process to the variable; otherwise, it does not.</param>
        /// <returns>The return value is the identifier of the thread that created the window.</returns>
        [DllImport(Consts.User32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetWindowThreadProcessId(IntPtr hwnd, out int processId);

        /// <summary>
        /// Determines whether the specified window handle identifies an existing window.
        /// </summary>
        /// <param name="hWnd">A handle to the window to be tested.</param>
        /// <returns>If the window handle identifies an existing window, the return value is nonzero.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        internal static extern bool IsWindow(IntPtr hWnd);

        /// <summary>
        /// Determines the visibility state of the specified window. 
        /// </summary>
        /// <param name="hWnd">A handle to the window to be tested. </param>
        /// <returns>If the specified window, its parent window, its parent's parent window, and so forth, have the WS_VISIBLE style, the return value is nonzero. Otherwise, the return value is zero.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        /// <summary>
        /// The MonitorFromPoint function retrieves a handle to the display monitor that contains a specified point.
        /// </summary>
        /// <param name="pt">A <see cref="System.Drawing.Point"/> structure that specifies the point of interest in virtual-screen coordinates.</param>
        /// <param name="dwFlags">Determines the function's return value if the point is not contained within any display monitor.</param>
        /// <returns>If the point is contained by a display monitor, the return value is an HMONITOR handle to that display monitor.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        internal static extern IntPtr MonitorFromPoint([In]System.Drawing.Point pt, [In]uint dwFlags);

        /// <summary>
        /// Removes the given window from the system-maintained clipboard format listener list.
        /// </summary>
        /// <param name="hwnd">A handle to the window to remove from the clipboard format listener list.</param>
        /// <returns>Returns TRUE if successful, FALSE otherwise.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        /// <summary>
        /// Sends the specified message to a window or windows. The SendMessage function calls the window procedure for the specified window and does not return until the window procedure has processed the message.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message. If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but the message is not sent to child windows.</param>
        /// <param name="wMsg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing; it depends on the message sent.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        internal static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

        /// <summary>
        /// Changes the parent window of the specified child window.
        /// </summary>
        /// <param name="hWndChild">A handle to the child window.</param>
        /// <param name="hWndNewParent">A handle to the new parent window. If this parameter is NULL, the desktop window becomes the new parent window. If this parameter is HWND_MESSAGE, the child window becomes a message-only window.</param>
        /// <returns>If the function succeeds, the return value is a handle to the previous parent window.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        internal static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        /// <summary>
        /// Sets various information regarding DWM window attributes
        /// </summary>
        /// <param name="hwnd">The window handle whose information is to be changed</param>
        /// <param name="data">Pointer to a structure which both specifies and delivers the attribute data</param>
        /// <returns>Nonzero on success, zero otherwise.</returns>
        [DllImport(Consts.User32)]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        /// <summary>
        /// Installs an application-defined hook procedure into a hook chain. You would install a hook procedure to monitor the system for certain types of events. These events are associated either with a specific thread or with all threads in the same desktop as the calling thread.
        /// </summary>
        /// <param name="hookType">The type of hook procedure to be installed.</param>
        /// <param name="lpfn">A pointer to the hook procedure. If the dwThreadId parameter is zero or specifies the identifier of a thread created by a different process, the lpfn parameter must point to a hook procedure in a DLL. Otherwise, lpfn can point to a hook procedure in the code associated with the current process.</param>
        /// <param name="hMod">A handle to the DLL containing the hook procedure pointed to by the lpfn parameter. The hMod parameter must be set to NULL if the dwThreadId parameter specifies a thread created by the current process and if the hook procedure is within the code associated with the current process.</param>
        /// <param name="dwThreadId">The identifier of the thread with which the hook procedure is to be associated. For desktop apps, if this parameter is zero, the hook procedure is associated with all existing threads running in the same desktop as the calling thread. For Windows Store apps, see the Remarks section.</param>
        /// <returns>If the function succeeds, the return value is the handle to the hook procedure.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(HookType hookType, HookProcCallback lpfn, IntPtr hMod, uint dwThreadId);

        /// <summary>
        /// Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function. 
        /// </summary>
        /// <param name="hhk">A handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to SetWindowsHookEx.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// Retrieves or sets the value of one of the system-wide parameters. This function can also update the user profile while setting a parameter.
        /// </summary>
        /// <param name="uiAction">The system-wide parameter to be retrieved or set.</param>
        /// <param name="uiParam">A parameter whose usage and format depends on the system parameter being queried or set. For more information about system-wide parameters, see the uiAction parameter. If not otherwise indicated, you must specify zero for this parameter.</param>
        /// <param name="pvParam">A parameter whose usage and format depends on the system parameter being queried or set.</param>
        /// <param name="fWinIni">If a system parameter is being set, specifies whether the user profile is to be updated, and if so, whether the WM_SETTINGCHANGE message is to be broadcast to all top-level windows to notify them of the change.</param>
        /// <returns>If the function succeeds, the return value is a nonzero value.</returns>
        [DllImport(Consts.User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref bool pvParam, uint fWinIni);

        #endregion

        #region UxTheme

        /// <summary>
        /// Get the immersive user color set preference.
        /// </summary>
        /// <param name="forceCheckRegistry"></param>
        /// <param name="skipCheckOnFail"></param>
        /// <returns></returns>
        [DllImport(Consts.UxTheme, SetLastError = true, EntryPoint = "#98", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        internal static extern uint GetImmersiveUserColorSetPreference(bool forceCheckRegistry, bool skipCheckOnFail);

        /// <summary>
        /// Get the immersive color set count.
        /// </summary>
        /// <returns></returns>
        [DllImport(Consts.UxTheme, SetLastError = true, EntryPoint = "#94", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        internal static extern uint GetImmersiveColorSetCount();

        /// <summary>
        /// Get the immersive color from color set.
        /// </summary>
        /// <param name="immersiveColorSet"></param>
        /// <param name="immersiveColorType"></param>
        /// <param name="ignoreHighContrast"></param>
        /// <param name="highContrastCacheMode"></param>
        /// <returns></returns>
        [DllImport(Consts.UxTheme, SetLastError = true, EntryPoint = "#95", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        internal static extern uint GetImmersiveColorFromColorSetEx(uint immersiveColorSet, uint immersiveColorType, bool ignoreHighContrast, uint highContrastCacheMode);

        /// <summary>
        /// Get the immersive color type from name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [DllImport(Consts.UxTheme, SetLastError = true, EntryPoint = "#96", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        internal static extern uint GetImmersiveColorTypeFromName(IntPtr name);

        /// <summary>
        /// Get the immersive color named type by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [DllImport(Consts.UxTheme, SetLastError = true, EntryPoint = "#100", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        internal static extern IntPtr GetImmersiveColorNamedTypeByIndex(uint index);

        #endregion

        #region Wininet

        /// <summary>
        /// Retrieves the connected state of the local system.
        /// </summary>
        /// <param name="lpdwFlags">Pointer to a variable that receives the connection description. This parameter may return a valid flag even when the function returns FALSE.</param>
        /// <param name="dwReserved">This parameter is reserved and must be 0.</param>
        /// <returns>Returns TRUE if there is an active modem or a LAN Internet connection, or FALSE if there is no Internet connection, or if all possible Internet connections are not currently active.</returns>
        [DllImport(Consts.Wininet)]
        internal static extern bool InternetGetConnectedState(out int lpdwFlags, int dwReserved);

        #endregion
    }
}
