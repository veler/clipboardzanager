using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using ClipboardZanager.Core.Desktop.Interop;
using ClipboardZanager.Shared.Logs;

namespace ClipboardZanager.Core.Desktop.ComponentModel
{
    /// <summary>
    /// Provides a set of methods used to get information about the application.
    /// </summary>
    internal static class CoreHelper
    {
        private static string _tempFolder;

        /// <summary>
        /// Get a value that defines whether the current execution is from a unit test
        /// </summary>
        /// <returns>True if a unit test is running</returns>
        internal static bool IsUnitTesting()
        {
            return Application.Current == null;
        }

        /// <summary>
        /// Returns the full path to the 
        /// </summary>
        /// <returns></returns>
        internal static string GetAppDataFolder()
        {
            if (IsUnitTesting())
            {
                if (!string.IsNullOrEmpty(_tempFolder))
                {
                    return _tempFolder;
                }

                var tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(tempFolder);
                _tempFolder = tempFolder;
                return tempFolder;
            }

            var appDataFolder = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            appDataFolder = Path.GetDirectoryName(Path.GetDirectoryName(appDataFolder));

            if (string.IsNullOrEmpty(appDataFolder))
            {
                Logger.Instance.Warning("Unable to retrieves the path to the application AppFolder.");
            }

            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            return appDataFolder;
        }

        /// <summary>
        /// Retrieves the handle of the current module.
        /// </summary>
        /// <returns>The handle of the current process's main module</returns>
        internal static IntPtr GetCurrentModuleHandle()
        {
            var currentProcess = Process.GetCurrentProcess();
            var module = currentProcess.MainModule;
            return NativeMethods.GetModuleHandle(module.ModuleName);
        }

        /// <summary>
        /// Returns the version of the executable
        /// </summary>
        /// <returns>A <see cref="Version"/> corresponding to the one of the executable.</returns>
        internal static Version GetApplicationVersion()
        {
            Assembly assembly;

            if (IsUnitTesting())
            {
                assembly = Assembly.GetExecutingAssembly();
            }
            else
            {
                assembly = Assembly.GetEntryAssembly();
            }

            return assembly.GetName().Version;
        }

        /// <summary>
        /// Gets application name.
        /// </summary>
        /// <returns>The application name.</returns>
        internal static string GetApplicationName()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                return assembly.GetName().Name;
            }

            return "UnitTestApp";
        }

        /// <summary>
        /// Create or delete the shortcut that makes the application starting with Windows.
        /// </summary>
        /// <param name="startWithWindows">Defines if the application must start with Windows.</param>
        internal static void UpdateStartWithWindowsShortcut(bool startWithWindows)
        {
            if (startWithWindows)
            {
                ShortcutHelper.CreateShortcut(Consts.StartWithWindowsShortcutFileName, true);
            }
            else if (File.Exists(Consts.StartWithWindowsShortcutFileName))
            {
                File.Delete(Consts.StartWithWindowsShortcutFileName);
            }
        }

        /// <summary>
        /// Try to minimize the application footprint
        /// </summary>
        internal static void MinimizeFootprint()
        {
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            NativeMethods.EmptyWorkingSet(Process.GetCurrentProcess().Handle);
        }

        /// <summary>
        /// Call Win32 NativeMethods.GlobalLock() with Win32 error checking.
        /// </summary>
        [SecurityCritical]
        internal static IntPtr Win32GlobalLock(HandleRef handle)
        {
            var win32Pointer = NativeMethods.GlobalLock(handle);
            var win32Error = Marshal.GetLastWin32Error();
            if (win32Pointer == IntPtr.Zero)
            {
                throw new Win32Exception(win32Error);
            }

            return win32Pointer;
        }

        /// <summary>
        /// Call Win32 NativeMethods.GlobalUnlock() with Win32 error checking.
        /// </summary>
        [SecurityCritical]
        internal static void Win32GlobalUnlock(HandleRef handle)
        {
            var win32Return = NativeMethods.GlobalUnlock(handle);
            var win32Error = Marshal.GetLastWin32Error();
            if (!win32Return && win32Error != 0)
            {
                throw new Win32Exception(win32Error);
            }
        }

        /// <summary>
        /// Call Win32 NativeMethods.GlobalSize() with Win32 error checking.
        /// </summary>
        [SecurityCritical]
        internal static IntPtr Win32GlobalSize(HandleRef handle)
        {
            var win32Pointer = NativeMethods.GlobalSize(handle);

            var win32Error = Marshal.GetLastWin32Error();
            if (win32Pointer == IntPtr.Zero)
            {
                throw new Win32Exception(win32Error);
            }

            return win32Pointer;
        }

        /// <summary>
        /// Call Win32 NativeMethods.GlobalAlloc() with Win32 error checking.
        /// </summary>
        [SecurityCritical]
        internal static IntPtr Win32GlobalAlloc(int flags, IntPtr bytes)
        {
            var win32Pointer = NativeMethods.GlobalAlloc(flags, bytes);
            var win32Error = Marshal.GetLastWin32Error();
            if (win32Pointer == IntPtr.Zero)
            {
                throw new Win32Exception(win32Error);
            }

            return win32Pointer;
        }

        /// <summary>
        /// Call Win32 NativeMethods.GlobalReAlloc() with Win32 error checking.
        /// </summary>
        [SecurityCritical]
        internal static IntPtr Win32GlobalReAlloc(HandleRef handle, IntPtr bytes, int flags)
        {
            var win32Pointer = NativeMethods.GlobalReAlloc(handle, bytes, flags);
            var win32Error = Marshal.GetLastWin32Error();
            if (win32Pointer == IntPtr.Zero)
            {
                throw new Win32Exception(win32Error);
            }

            return win32Pointer;
        }

        /// <summary>
        /// Call Win32 NativeMethods.GlobalFree() with Win32 error checking.
        /// </summary>
        [SecurityCritical]
        internal static void Win32GlobalFree(HandleRef handle)
        {
            var win32Pointer = NativeMethods.GlobalFree(handle);
            var win32Error = Marshal.GetLastWin32Error();
            if (win32Pointer != IntPtr.Zero)
            {
                throw new Win32Exception(win32Error);
            }
        }

        /// <summary>
        /// Ensures that a memory block is sized to match a specified byte count.
        /// </summary>
        /// <remarks>
        /// Returns a pointer to the original memory block, a re-sized memory block,
        /// or null if the original block has insufficient capacity and doNotReallocate
        /// is true.
        ///
        /// Returns an HRESULT
        ///  S_OK: success.
        ///  STG_E_MEDIUMFULL: the original handle lacks capacity and doNotReallocate == true.  handle == null on exit.
        ///  E_OUTOFMEMORY: could not re-size the handle.  handle == null on exit.
        ///
        /// If doNotReallocate is false, this method will always realloc the original
        /// handle to fit minimumByteCount tightly.
        /// </remarks>
        [SecurityCritical]
        internal static int EnsureMemoryCapacity(ref IntPtr handle, HandleRef handleRef, int minimumByteCount, bool doNotReallocate)
        {
            if (doNotReallocate)
            {
                var byteCount = DataHelper.IntPtrToInt32(Win32GlobalSize(handleRef));
                if (byteCount < minimumByteCount)
                {
                    handle = IntPtr.Zero;
                    return Consts.StgEMediumfull;
                }
            }
            else
            {
                handle = Win32GlobalReAlloc(handleRef, (IntPtr)minimumByteCount, Consts.GMEM_MOVEABLE | Consts.GMEM_DDESHARE | Consts.GMEM_ZEROINIT);

                if (handle == IntPtr.Zero)
                {
                    return Consts.E_OUTOFMEMORY;
                }
            }

            return Consts.S_OK;
        }
    }
}
