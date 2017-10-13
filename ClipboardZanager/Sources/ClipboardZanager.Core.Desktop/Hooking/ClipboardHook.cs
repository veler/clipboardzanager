using System;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Events;
using ClipboardZanager.Core.Desktop.Interop;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;

namespace ClipboardZanager.Core.Desktop.Hooking
{
    /// <summary>
    /// Provides a set of functions designed to listen to the clipboard.
    /// </summary>
    internal sealed class ClipboardHook : IPausable
    {
        #region Fields

        private bool _isPaused;
        private readonly HwndSource _hWndSource;

        #endregion

        #region Events

        /// <summary>
        /// Raised when the clipboard content has changed.
        /// </summary>
        internal event EventHandler<ClipboardHookEventArgs> ClipboardChanged;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="ClipboardHook"/> class.
        /// </summary>
        /// <param name="window">The window that host the clipboard hooking.</param>
        internal ClipboardHook(Window window)
        {
            DispatcherHelper.ThrowIfNotStaThread();

            var windowHandle = new WindowInteropHelper(window).EnsureHandle();
            if (windowHandle == IntPtr.Zero)
            {
                Logger.Instance.Fatal(new Exception("Unable to start the clipboard hook because the given window has no handle."));
            }

            _hWndSource = HwndSource.FromHwnd(windowHandle);

            _isPaused = true;
            Resume();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void Pause()
        {
            if (!_isPaused)
            {
                DispatcherHelper.ThrowIfNotStaThread();
                SecurityHelper.DemandAllClipboardPermission();
                if (NativeMethods.RemoveClipboardFormatListener(_hWndSource.Handle))
                {
                    Logger.Instance.Information("Clipboard hooking stopped.");
                }
                else
                {
                    Logger.Instance.Error(new Exception("Clipboard hooking failed to stop."));
                }

                _hWndSource.RemoveHook(WndProc);
                _isPaused = true;
            }
        }

        /// <inheritdoc/>
        public void Resume()
        {
            if (_isPaused)
            {
                DispatcherHelper.ThrowIfNotStaThread();
                SecurityHelper.DemandAllClipboardPermission();
                if (NativeMethods.SetParent(_hWndSource.Handle, Consts.HwndMessage) == IntPtr.Zero)
                {
                    Logger.Instance.Fatal(new Exception("Clipboard hooking failed to set the parent window of the one used to listen to the system events."));
                }

                if (NativeMethods.AddClipboardFormatListener(_hWndSource.Handle))
                {
                    Logger.Instance.Information("Clipboard hooking started.");
                }
                else
                {
                    Logger.Instance.Fatal(new Exception("Clipboard hooking failed to add the clipboard format listener."));
                }

                _hWndSource.AddHook(WndProc);
                _isPaused = false;
            }
        }

        /// <summary>
        /// An application-defined function that processes messages sent to a window. The WNDPROC type defines a pointer to this callback function.
        /// </summary>
        /// <param name="hwnd">A handle to the window.</param>
        /// <param name="msg">The message.</param>
        /// <param name="wParam">Additional message information. The contents of this parameter depend on the value of the <param name="msg"/> parameter.</param>
        /// <param name="lParam">Additional message information. The contents of this parameter depend on the value of the <param name="msg"/> parameter.</param>
        /// <param name="handled"></param>
        /// <returns>The return value is the result of the message processing and depends on the message sent.</returns>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != Consts.ClipboardUpdateMessage)
            {
                return IntPtr.Zero;
            }

            try
            {
                var data = System.Windows.Clipboard.GetDataObject() as DataObject;
                var isCut = false;

                if (data == null)
                {
                    return IntPtr.Zero;
                }

                if (data.GetDataPresent(Consts.DropEffectFormatName))
                {
                    var preferredDropEffect = data.GetData(Consts.DropEffectFormatName) as System.IO.MemoryStream;
                    if (preferredDropEffect != null)
                    {
                        isCut = preferredDropEffect.ToArray().SequenceEqual(new byte[] { 2, 0, 0, 0 });
                    }
                }

                Logger.Instance.Information("New data detected into the Windows clipboard.");
                ClipboardChanged?.Invoke(this, new ClipboardHookEventArgs(data, isCut, DateTime.Now.Ticks));
            }
            catch (Exception ex)
            {
                Logger.Instance.Warning($"A data has been copied by the user, but the app is not able to retrieve it.");
                Logger.Instance.Error(ex);
            }

            return IntPtr.Zero;
        }

        #endregion

        #region Destructors

        /// <summary>
        /// Explicit destructor of the class.
        /// </summary>
        ~ClipboardHook()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose class and unhook keyboard hook.
        /// </summary>
        public void Dispose()
        {
            Pause();
        }

        #endregion
    }
}
