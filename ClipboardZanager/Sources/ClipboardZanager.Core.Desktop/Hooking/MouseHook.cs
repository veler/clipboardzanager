using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Events;
using ClipboardZanager.Core.Desktop.Interop;
using ClipboardZanager.Core.Desktop.Interop.Structs;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;
using static ClipboardZanager.Core.Desktop.ComponentModel.Delegates;

namespace ClipboardZanager.Core.Desktop.Hooking
{
    /// <summary>
    /// Provides the possibility to listen to the mouse.
    /// </summary>
    internal sealed class MouseHook : IPausable
    {
        #region Fields

        private readonly HookProcCallback _mouseHookProcHandle;

        private IntPtr _mouseHookHandle;
        private Point _lastMouseCoords;

        private int _historyLimit;
        private bool _isPaused;

        #endregion

        #region Properties

        /// <summary>
        /// Determine whether the library is to intercept mouse button up.
        /// </summary>
        internal bool HandleMouseButtonUp { get; set; }

        /// <summary>
        /// Determine whether the library is to intercept mouse button down.
        /// </summary>
        internal bool HandleMouseButtonDown { get; set; }

        /// <summary>
        /// Determine whether the library is to intercept mouse button move.
        /// </summary>
        internal bool HandleMouseMove { get; set; }

        /// <summary>
        /// Determine whether the library is to intercept mouse wheel.
        /// </summary>
        internal bool HandleMouseWheel { get; set; }

        /// <summary>
        /// List of called events. Limit of items can be set in HistoryLimit var.
        /// </summary>
        internal List<MouseHookEventArgs> EventsHistory { get; set; }

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

        /// <summary>
        /// Gets the current mouse coordonates.
        /// </summary>
        internal Point MouseCoords { get; private set; }

        /// <summary>
        /// Gets the state of the mouse left button.
        /// </summary>
        internal KeyState LeftMouseButtonState { get; private set; }

        /// <summary>
        /// Gets the state of the mouse right button.
        /// </summary>
        internal KeyState RightMouseButtonState { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Raised when a mouse movement or click is detected.
        /// </summary>
        internal event EventHandler<MouseHookEventArgs> MouseAction;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="MouseHook"/> class.
        /// </summary>
        internal MouseHook()
        {
            _lastMouseCoords = new Point();
            NativeMethods.GetCursorPos(ref _lastMouseCoords);

            _mouseHookProcHandle = MouseHookProc;

            _isPaused = true;
            Resume();

            EventsHistory = new List<MouseHookEventArgs>();
            LeftMouseButtonState = KeyState.Released;
            RightMouseButtonState = KeyState.Released;
            HistoryLimit = 100;
            HandleMouseButtonDown = true;
            HandleMouseButtonUp = true;
            HandleMouseMove = true;
            HandleMouseWheel = true;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void Pause()
        {
            if (!_isPaused)
            {
                //Unhook mouse hook
                NativeMethods.UnhookWindowsHookEx(_mouseHookHandle);
                _isPaused = true;

                Logger.Instance.Information("Mouse hooking stopped.");
            }
        }

        /// <inheritdoc/>
        public void Resume()
        {
            if (_isPaused)
            {
                // Register mouse hook
                _mouseHookHandle = NativeMethods.SetWindowsHookEx(HookType.MouseLl, _mouseHookProcHandle, CoreHelper.GetCurrentModuleHandle(), 0);
                _isPaused = false;

                // If returned handle is smaller than 0, throw exception
                if ((int)_mouseHookHandle == 0)
                {
                    Logger.Instance.Fatal(new Exception("Mouse hooking failed to start : returned keyboard hook handle is null"));
                }
                else
                {
                    Logger.Instance.Information("Mouse hooking started.");
                }
            }
        }

        private IntPtr MouseHookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            // If code is smaller than 0, this is error
            if (code < 0)
            {
                return NativeMethods.CallNextHookEx(_mouseHookHandle, code, wParam, lParam);
            }

            var mouseAction = (MouseSignal)wParam;
            var mouseData = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
            var mouseWheelData = (int)mouseData.mouseData >> 16;
            var wheelAction = MouseWheelAction.Unknown;

            if (mouseAction == MouseSignal.MouseWheel)
            {
                if (mouseWheelData > 0)
                {
                    wheelAction = MouseWheelAction.WheelUp;
                }
                else
                {
                    wheelAction = MouseWheelAction.WheelDown;
                }
            }

            //Get mouse coords from point
            MouseCoords = mouseData.pt;

            //Calculate mouse coords delta
            var delta = new Point
            {
                X = mouseData.pt.X - _lastMouseCoords.X,
                Y = mouseData.pt.Y - _lastMouseCoords.Y
            };

            //Update last mouse coords
            _lastMouseCoords = mouseData.pt;

            //Init MouseAction enum
            var action = new MouseAction();

            //Set action from mouseAction (wParam)
            if (mouseAction == MouseSignal.LButtonDown)
            {
                action = Enums.MouseAction.LeftButtonPressed;
                LeftMouseButtonState = KeyState.Pressed;
            }
            else if (mouseAction == MouseSignal.LButtonUp)
            {
                action = Enums.MouseAction.LeftButtonReleased;
                LeftMouseButtonState = KeyState.Released;
            }
            else if (mouseAction == MouseSignal.RButtonDown)
            {
                action = Enums.MouseAction.RightButtonPressed;
                RightMouseButtonState = KeyState.Pressed;
            }
            else if (mouseAction == MouseSignal.RButtonUp)
            {
                action = Enums.MouseAction.RightButtonReleased;
                RightMouseButtonState = KeyState.Released;
            }
            else if (mouseAction == MouseSignal.MouseMove)
            {
                action = Enums.MouseAction.MouseMove;
            }
            else if (mouseAction == MouseSignal.MouseWheel)
            {
                action = Enums.MouseAction.MouseWheel;
            }

            //If this action is allowed, go next
            if (IsActionAllowed(mouseAction))
            {
                var eventArgs = new MouseHookEventArgs(mouseData.pt, delta, action, wheelAction, DateTime.Now.Ticks);

                AddEventToHistory(eventArgs);

                MouseAction?.Invoke(this, eventArgs);

                if (eventArgs.Handled)
                {
                    return (IntPtr)1;
                }
            }

            return NativeMethods.CallNextHookEx(_mouseHookHandle, code, wParam, lParam);
        }

        /// <summary>
        /// Check if the current mouse action if allowed.
        /// </summary>
        /// <param name="action">The state of the mouse.</param>
        /// <returns>True if it is allowed.</returns>
        private bool IsActionAllowed(MouseSignal action)
        {
            if (((action == MouseSignal.LButtonDown || action == MouseSignal.RButtonDown) && HandleMouseButtonDown) ||
                ((action == MouseSignal.LButtonUp || action == MouseSignal.RButtonUp) && HandleMouseButtonUp) ||
                ((action == MouseSignal.MouseMove) && HandleMouseMove) ||
                ((action == MouseSignal.MouseWheel) && HandleMouseWheel))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add the specified event to the history.
        /// </summary>
        /// <param name="eventArgs">The <see cref="MouseHookEventArgs"/>.</param>
        private void AddEventToHistory(MouseHookEventArgs eventArgs)
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
        ~MouseHook()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose and unhook all hooks.
        /// </summary>
        public void Dispose()
        {
            Pause();
        }

        #endregion
    }
}
