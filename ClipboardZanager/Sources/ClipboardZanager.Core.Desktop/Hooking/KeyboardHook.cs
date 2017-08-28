using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Events;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Core.Desktop.Interop;
using ClipboardZanager.Shared.Logs;
using static ClipboardZanager.Core.Desktop.ComponentModel.Delegates;

namespace ClipboardZanager.Core.Desktop.Hooking
{
    /// <summary>
    /// Provides the possibility to listen to the keyboard.
    /// </summary>
    internal sealed class KeyboardHook : IPausable
    {
        #region Fields

        private readonly HookProcCallback _keyboardHookProcHandle;

        private IntPtr _keyboardHookHandle;
        private int _historyLimit = 100;
        private bool _isPaused;

        #endregion

        #region Properties

        /// <summary>
        /// Determine whether the library is to intercept key press.
        /// </summary>
        internal bool HandleKeyboardKeyPress { get; set; }

        /// <summary>
        /// Determine whether the library is to intercept key release.
        /// </summary>
        internal bool HandleKeyboardKeyRelease { get; set; }

        /// <summary>
        /// List of called events. Limit of items can be set in HistoryLimit var.
        /// </summary>
        internal List<KeyboardHookEventArgs> EventsHistory { get; set; }

        /// <summary>
        /// Limit of items in called event history. If 0, history can be empty.
        /// </summary>
        internal int HistoryLimit
        {
            get { return _historyLimit; }
            set
            {
                _historyLimit = value;
                if (EventsHistory.Count >= HistoryLimit)
                {
                    EventsHistory.RemoveRange(HistoryLimit, EventsHistory.Count - HistoryLimit);
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when a key of the keyboard is detected.
        /// </summary>
        internal event EventHandler<KeyboardHookEventArgs> KeyboardAction;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="KeyboardHook"/> class.
        /// </summary>
        internal KeyboardHook()
        {
            _keyboardHookProcHandle = KeyboardHookProc;

            _isPaused = true;
            Resume();

            EventsHistory = new List<KeyboardHookEventArgs>();
            HistoryLimit = 100;
            HandleKeyboardKeyPress = true;
            HandleKeyboardKeyRelease = true;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void Pause()
        {
            if (!_isPaused)
            {
                // Unhook keyboard hook
                NativeMethods.UnhookWindowsHookEx(_keyboardHookHandle);
                _isPaused = true;

                Logger.Instance.Information("Keyboard hooking stopped.");
            }
        }

        /// <inheritdoc/>
        public void Resume()
        {
            if (_isPaused)
            {
                // Register keyboard hook
                _keyboardHookHandle = NativeMethods.SetWindowsHookEx(HookType.KeyboardLl, _keyboardHookProcHandle, CoreHelper.GetCurrentModuleHandle(), 0);
                _isPaused = false;

                // If returned handle is equal to 0, throw exception
                if ((int)_keyboardHookHandle == 0)
                {
                    Logger.Instance.Fatal(new Exception("Keyboard hooking failed to start : returned keyboard hook handle is null"));
                }
                else
                {
                    Logger.Instance.Information("Keyboard hooking started.");
                }
            }
        }

        /// <summary>
        /// Called when a key is pressed or released.
        /// </summary>
        /// <param name="code">A code the hook procedure uses to determine how to process the message. If code is less than zero, the hook procedure must pass the message to the CallNextHookEx function without further processing and should return the value returned by CallNextHookEx.</param>
        /// <param name="wParam">The virtual-key code of the key that generated the keystroke message.</param>
        /// <param name="lParam">The repeat count, scan code, extended-key flag, context code, previous key-state flag, and transition-state flag. For more information about the lParam parameter, see Keystroke Message Flags.</param>
        /// <returns>If code is less than zero, the hook procedure must return the value returned by CallNextHookEx.</returns>
        private IntPtr KeyboardHookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
            {
                return NativeMethods.CallNextHookEx(_keyboardHookHandle, code, wParam, lParam);
            }

            var vKey = Marshal.ReadInt32(lParam);
            var key = KeyInterop.KeyFromVirtualKey(vKey);

            var keyState = KeyState.Unknown;

            if ((int)wParam == 256 || (int)wParam == 260)
            {
                keyState = KeyState.Pressed;
            }
            else if ((int)wParam == 257)
            {
                keyState = KeyState.Released;
            }

            // Get capslock and shifts states, and determine whether the key is uppercase or not
            var capslockState = Convert.ToBoolean(NativeMethods.GetKeyState(0x14));
            var lShiftState = Convert.ToBoolean(NativeMethods.GetKeyState(0xA0) >> 1);
            var rShiftState = Convert.ToBoolean(NativeMethods.GetKeyState(0xA1) >> 1);
            if (rShiftState || lShiftState)
            {
                capslockState = !capslockState;
            }

            if (IsActionAllowed(keyState))
            {
                var eventArgs = new KeyboardHookEventArgs(key, keyState, capslockState, DateTime.Now.Ticks);

                AddEventToHistory(eventArgs);

                KeyboardAction?.Invoke(this, eventArgs);

                if (eventArgs.Handled)
                {
                    return (IntPtr)1;
                }
            }

            return NativeMethods.CallNextHookEx(_keyboardHookHandle, code, wParam, lParam);
        }

        /// <summary>
        /// Check if the current key action if allowed.
        /// </summary>
        /// <param name="action">The state of the key.</param>
        /// <returns>True if it is allowed.</returns>
        private bool IsActionAllowed(KeyState action)
        {
            if (((action == KeyState.Pressed) && HandleKeyboardKeyPress) || ((action == KeyState.Released) && HandleKeyboardKeyRelease))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add the specified event to the history.
        /// </summary>
        /// <param name="eventArgs">The <see cref="KeyboardHookEventArgs"/>.</param>
        private void AddEventToHistory(KeyboardHookEventArgs eventArgs)
        {
            if (HistoryLimit != 0)
            {
                if (EventsHistory.Count >= HistoryLimit && EventsHistory.Any())
                {
                    EventsHistory.RemoveAt(EventsHistory.Count - 1);
                }

                EventsHistory.Insert(0, eventArgs);
            }
        }

        #endregion

        #region Destructors

        /// <summary>
        /// Explicit destructor of the class.
        /// </summary>
        ~KeyboardHook()
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
