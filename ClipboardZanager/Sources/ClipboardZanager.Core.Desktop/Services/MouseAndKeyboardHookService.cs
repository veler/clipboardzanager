using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Events;
using ClipboardZanager.Core.Desktop.Hooking;
using ClipboardZanager.Core.Desktop.Interop.Structs;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Shared.Services;

namespace ClipboardZanager.Core.Desktop.Services
{
    /// <summary>
    /// Provides a set of functions designed to listen to the mouse and keyboard and detect shortcuts and gestures.
    /// </summary>
    internal sealed class MouseAndKeyboardHookService : IService, IPausable
    {
        #region Fields

        private KeyboardHook _keyboardHook;
        private MouseHook _mouseHook;
        private List<Point> _mousePointsList;
        private List<Gesture> _gesturesList;
        private List<Tuple<DateTime, Key>> _clickedKeys;
        private List<HotKey> _hotKeys;

        private int _historyLimit;
        private bool _inPause;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value that defines wether the service is in pause of not.
        /// </summary>
        internal bool IsPaused => _inPause;

        /// <summary>
        /// Determine tolerance for gestures. If value is highter, a gesture must be done more precisely
        /// </summary>
        internal int GestureTolerance { get; set; }

        /// <summary>
        /// Determine whether user is acutally making gesture
        /// </summary>
        internal bool IsGestureMaking { get; set; }

        /// <summary>
        /// Determine which mouse button you will be responsible for the initiation and termination of a mouse gesture.
        /// </summary>
        internal MouseButtons GestureActionButton { get; set; }

        /// <summary>
        /// Determine whether the library is to intercept key release
        /// </summary>
        internal bool HandleHotKeyPress
        {
            get
            {
                return _keyboardHook.HandleKeyboardKeyPress;
            }
            set
            {
                _keyboardHook.HandleKeyboardKeyPress = value;
            }
        }

        /// <summary>
        /// Determine whether the library is to intercept key release
        /// </summary>
        internal bool HandleHotKeyRelease
        {
            get
            {
                return _keyboardHook.HandleKeyboardKeyRelease;
            }
            set
            {
                _keyboardHook.HandleKeyboardKeyRelease = value;
            }
        }

        /// <summary>
        /// Determine whether the library is to intercept mouse button up
        /// </summary>
        internal bool HandleMouseButtonUp
        {
            get
            {
                return _mouseHook.HandleMouseButtonUp;
            }
            set
            {
                _mouseHook.HandleMouseButtonUp = value;
            }
        }

        /// <summary>
        /// Determine whether the library is to intercept mouse button down
        /// </summary>
        internal bool HandleMouseButtonDown
        {
            get
            {
                return _mouseHook.HandleMouseButtonDown;
            }
            set
            {
                _mouseHook.HandleMouseButtonDown = value;
            }
        }

        /// <summary>
        /// Determine whether the library is to intercept mouse button move
        /// </summary>
        internal bool HandleMouseMove
        {
            get
            {
                return _mouseHook.HandleMouseMove;
            }
            set
            {
                _mouseHook.HandleMouseMove = value;
            }
        }

        /// <summary>
        /// Determine whether the library is to intercept mouse wheel
        /// </summary>
        internal bool HandleMouseWheel
        {
            get
            {
                return _mouseHook.HandleMouseWheel;
            }
            set
            {
                _mouseHook.HandleMouseWheel = value;
            }
        }

        /// <summary>
        /// List of called mouse events. Limit of items can be set in HistoryLimit var
        /// </summary>
        internal List<MouseGestureEventArgs> MouseGestureEventsHistory { get; set; }

        /// <summary>
        /// List of called keyboard events. Limit of items can be set in HistoryLimit var
        /// </summary>
        internal List<HotKeyEventArgs> HotKeysEventsHistory { get; set; }

        /// <summary>
        /// Limit of items in called event history. If 0, history can be empty
        /// </summary>
        internal int HistoryLimit
        {
            get { return _historyLimit; }
            set
            {
                _historyLimit = value;
                if (MouseGestureEventsHistory.Count >= HistoryLimit)
                {
                    MouseGestureEventsHistory.RemoveRange(HistoryLimit, MouseGestureEventsHistory.Count - HistoryLimit);
                }
                if (HotKeysEventsHistory.Count >= HistoryLimit)
                {
                    HotKeysEventsHistory.RemoveRange(HistoryLimit, HotKeysEventsHistory.Count - HistoryLimit);
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when a mouse gesture is detected.
        /// </summary>
        internal event EventHandler<MouseGestureEventArgs> MouseGestureDetected;

        /// <summary>
        /// Raised when a hot key is detected.
        /// </summary>
        internal event EventHandler<HotKeyEventArgs> HotKeyDetected;

        /// <summary>
        /// Raised when a mouse movement or click is detected.
        /// </summary>
        internal event EventHandler<MouseHookEventArgs> MouseAction;

        /// <summary>
        /// Raised when a key of the keyboard is detected.
        /// </summary>
        internal event EventHandler<KeyboardHookEventArgs> KeyboardAction;

        #endregion

        #region Methods 

        /// <inheritdoc/>
        public void Initialize(IServiceSettingProvider serviceSetting)
        {
            DispatcherHelper.ThrowIfNotStaThread();

            _mousePointsList = new List<Point>();
            _gesturesList = new List<Gesture>();
            _clickedKeys = new List<Tuple<DateTime, Key>>();
            _hotKeys = new List<HotKey>();

            _keyboardHook = new KeyboardHook();
            _mouseHook = new MouseHook();
            _inPause = true;

            MouseGestureEventsHistory = new List<MouseGestureEventArgs>();
            HotKeysEventsHistory = new List<HotKeyEventArgs>();
            GestureTolerance = 10;
            HistoryLimit = 100;
            HandleHotKeyPress = true;
            HandleHotKeyRelease = true;
            HandleMouseButtonDown = true;
            HandleMouseButtonUp = true;
            HandleMouseMove = true;
            HandleMouseWheel = true;
            GestureActionButton = MouseButtons.LeftMouseButton;

            Resume();

            Logger.Instance.Information($"{GetType().Name} initialized.");
        }

        /// <inheritdoc/>
        public void Reset()
        {
            Pause();
            _mousePointsList.Clear();
            _gesturesList.Clear();
            _clickedKeys.Clear();
            _hotKeys.Clear();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Pause();
            _mouseHook.Dispose();
            _keyboardHook.Dispose();
        }

        /// <inheritdoc/>
        public void Pause()
        {
            if (!_inPause)
            {
                _inPause = true;

                if (_keyboardHook != null && _mouseHook != null)
                {
                    _keyboardHook.KeyboardAction -= OnKeyboardAction;
                    _mouseHook.MouseAction -= OnMouseAction;
                    _keyboardHook.Pause();
                    _mouseHook.Pause();
                }
            }
        }

        /// <inheritdoc/>
        public void Resume()
        {
            if (_inPause)
            {
                _inPause = false;
                HotKeysEventsHistory?.Clear();
                MouseGestureEventsHistory?.Clear();

                if (_keyboardHook != null && _mouseHook != null)
                {
                    _keyboardHook.Resume();
                    _mouseHook.Resume();
                    _keyboardHook.KeyboardAction += OnKeyboardAction;
                    _mouseHook.MouseAction += OnMouseAction;
                }
            }
        }

        /// <summary>
        /// Resume the hooking with the specified delay.
        /// </summary>
        /// <param name="delay">The delay before resuming.</param>
        internal void DelayedResume(TimeSpan delay)
        {
            var delayedHooking = new Delayer<object>(delay);
            delayedHooking.Action += (sender, args) => Resume();
            delayedHooking.ResetAndTick();
        }

        /// <summary>
        /// Register a new hot key with a specific name
        /// </summary>
        /// <param name="name">Specific name to hot key</param>
        /// <param name="keys">List of keys</param>
        internal void RegisterHotKey(string name, params Key[] keys)
        {
            Requires.NotNullOrWhiteSpace(name, nameof(name));

            if (_hotKeys.Exists(p => string.Equals(p.Name, name, StringComparison.Ordinal)))
            {
                Logger.Instance.Warning("HotKey name already exist");
            }
            else
            {
                var hotKey = new HotKey
                {
                    Name = name,
                    Keys = keys.ToList()
                };

                _hotKeys.Add(hotKey);
            }
        }

        /// <summary>
        /// Unregister hot key with a specific name
        /// </summary>
        /// <param name="name">Name of hot key</param>
        internal void UnregisterHotKey(string name)
        {
            Requires.NotNullOrWhiteSpace(name, nameof(name));

            if (_hotKeys.Exists(p => string.Equals(p.Name, name, StringComparison.Ordinal)))
            {
                _hotKeys.RemoveAll(p => string.Equals(p.Name, name, StringComparison.Ordinal));
            }
        }

        /// <summary>
        /// Register new gesture
        /// </summary>
        /// <param name="name">Name of the gesture</param>
        /// <param name="checkPoints">Points, that are used in analyse gesture</param>
        internal void RegisterGesture(string name, bool[,] checkPoints)
        {
            Requires.NotNullOrWhiteSpace(name, nameof(name));

            if (_gesturesList.Exists(p => string.Equals(p.Name, name, StringComparison.Ordinal)))
            {
                Logger.Instance.Warning("Gesture name is already registred");
            }
            else
            {
                var xLenght = checkPoints.GetLength(0);
                var yLenght = checkPoints.GetLength(1);
                var convertedCheckPoints = new int[xLenght, yLenght];

                for (var x = 0; x < xLenght; x++)
                {
                    for (var y = 0; y < yLenght; y++)
                    {
                        convertedCheckPoints[x, y] = Convert.ToInt32(checkPoints[x, y]);
                    }
                }

                var gesture = new Gesture
                {
                    Name = name,
                    CheckPoinstArray = convertedCheckPoints
                };

                _gesturesList.Add(gesture);
            }
        }

        /// <summary>
        /// Register new gesture
        /// </summary>
        /// <param name="name">Name of the gesture</param>
        /// <param name="checkPoints">Points, that are used in analyse gesture</param>
        internal void RegisterGesture(string name, int[,] checkPoints)
        {
            Requires.NotNullOrWhiteSpace(name, nameof(name));

            if (_gesturesList.Exists(p => string.Equals(p.Name, name, StringComparison.Ordinal)))
            {
                Logger.Instance.Warning("Gesture name is already registred");
            }
            else
            {
                var xLenght = checkPoints.GetLength(0);
                var yLenght = checkPoints.GetLength(1);

                var gesture = new Gesture
                {
                    Name = name,
                    CheckPoinstArray = checkPoints
                };

                _gesturesList.Add(gesture);
            }
        }

        /// <summary>
        /// Unregister gesture
        /// </summary>
        /// <param name="name">Name of the gesture</param>
        internal void UnregisterGesture(string name)
        {
            Requires.NotNullOrWhiteSpace(name, nameof(name));

            if (_gesturesList.Exists(p => string.Equals(p.Name, name, StringComparison.Ordinal)))
            {
                _gesturesList.RemoveAll(p => string.Equals(p.Name, name, StringComparison.Ordinal));
            }
        }

        /// <summary>
        /// Detect a mouse gesture
        /// </summary>
        /// <returns>The <see cref="Gesture"/>. If no gesture has been detected, a gesture named "-1" is returned.</returns>
        private Gesture DetectGesture()
        {
            // Get min-max values from mouse coords list
            var gestureMaxX = _mousePointsList.Max(p => p.X);
            var gestureMinX = _mousePointsList.Min(p => p.X);
            var gestureMaxY = _mousePointsList.Max(p => p.Y);
            var gestureMinY = _mousePointsList.Min(p => p.Y);

            // Get area of gesture
            var gestureAreaX = gestureMaxX - gestureMinX;
            var gestureAreaY = gestureMaxY - gestureMinY;

            for (var i = 0; i < _gesturesList.Count; i++)
            {
                // Get size of Check Points array
                var xLenght = _gesturesList[i].CheckPoinstArray.GetLength(0);
                var yLenght = _gesturesList[i].CheckPoinstArray.GetLength(1);

                // Get size of single field
                var singleFieldX = gestureAreaX / xLenght;
                var singleFieldY = gestureAreaY / yLenght;

                var currentFieldIndex = -1;

                // Single field must be highter than 5
                if (singleFieldX > 5 && singleFieldY > 5)
                {
                    var fixedGestureArray = new bool[xLenght, yLenght];
                    var goodCheckPoints = 0;
                    var badCheckPoints = 0;

                    for (var x = 0; x < _mousePointsList.Count; x++)
                    {
                        // Get index of fields, where mouse coord is
                        var fieldX = (int)Math.Floor((double)(_mousePointsList[x].X - gestureMinX) / singleFieldX);
                        var fieldY = (int)Math.Floor((double)(_mousePointsList[x].Y - gestureMinY) / singleFieldY);

                        // Indexes can't be highter than size of array
                        if (fieldX >= xLenght)
                        {
                            fieldX = xLenght - 1;
                        }

                        if (fieldY >= yLenght)
                        {
                            fieldY = yLenght - 1;
                        }

                        if (_gesturesList[i].CheckPoinstArray[fieldY, fieldX] < currentFieldIndex && _gesturesList[i].CheckPoinstArray[fieldY, fieldX] != 0)
                        {
                            break;
                        }
                        else
                        {
                            currentFieldIndex = _gesturesList[i].CheckPoinstArray[fieldY, fieldX];
                        }

                        fixedGestureArray[fieldY, fieldX] = true;
                    }

                    for (var x = 0; x < xLenght; x++)
                    {
                        for (var y = 0; y < yLenght; y++)
                        {
                            if (fixedGestureArray[x, y] && _gesturesList[i].CheckPoinstArray[x, y] != 0)
                            {
                                goodCheckPoints++;
                            }
                            else if ((fixedGestureArray[x, y] && _gesturesList[i].CheckPoinstArray[x, y] == 0) || (!fixedGestureArray[x, y] && _gesturesList[i].CheckPoinstArray[x, y] != 0))
                            {
                                badCheckPoints++;
                            }
                        }
                    }

                    var totalfields = xLenght * yLenght;
                    var badPercents = badCheckPoints * 100 / totalfields;

                    if (badPercents <= GestureTolerance)
                    {
                        return _gesturesList[i];
                    }
                }
            }

            return new Gesture
            {
                Name = "-1",
                CheckPoinstArray = null
            };
        }

        /// <summary>
        /// Detect hot keys
        /// </summary>
        /// <returns>The list of detected hot keys</returns>
        private List<HotKey> DetectHotKeys()
        {
            var hotKeys = new List<HotKey>();

            if (!_clickedKeys.Any())
            {
                return hotKeys;
            }

            // List of hot keys
            for (var x = 0; x < _hotKeys.Count; x++)
            {
                // Init isHotKey flag
                var isHotKey = true;

                // If clicked keys count is smaller than keys in hot key, skip this
                if (_hotKeys[x].Keys.Count != _clickedKeys.Count)
                {
                    continue;
                }

                // List of clicked keys
                for (var y = 0; y < _clickedKeys.Count; y++)
                {
                    // If clicked key isn't in list of keys in hot key, skip this
                    if (_hotKeys[x].Keys[y] != _clickedKeys[y].Item2)
                    {
                        isHotKey = false;
                        break;
                    }
                }

                // If processed hot key is called
                if (isHotKey)
                {
                    // Add new hot key to the list
                    hotKeys.Add(new HotKey
                    {
                        Name = _hotKeys[x].Name,
                        Keys = _hotKeys[x].Keys,
                    });
                }
            }

            return hotKeys;
        }

        /// <summary>
        /// Add the specified event to the history
        /// </summary>
        /// <param name="eventArgs">The <see cref="MouseGestureEventArgs"/></param>
        private void AddEventToHistory(MouseGestureEventArgs eventArgs)
        {
            if (HistoryLimit == 0)
            {
                return;
            }

            Requires.NotNull(eventArgs, nameof(eventArgs));

            if (MouseGestureEventsHistory.Count >= HistoryLimit && MouseGestureEventsHistory.Count > 0)
            {
                MouseGestureEventsHistory.RemoveAt(MouseGestureEventsHistory.Count - 1);
            }

            MouseGestureEventsHistory.Insert(0, eventArgs);
        }

        /// <summary>
        /// Add the specified event to the history
        /// </summary>
        /// <param name="eventArgs">The <see cref="HotKeyEventArgs"/></param>
        private void AddEventToHistory(HotKeyEventArgs eventArgs)
        {
            if (HistoryLimit == 0)
            {
                return;
            }

            Requires.NotNull(eventArgs, nameof(eventArgs));

            if (HotKeysEventsHistory.Count >= HistoryLimit && HotKeysEventsHistory.Count > 0)
            {
                HotKeysEventsHistory.RemoveAt(HotKeysEventsHistory.Count - 1);
            }

            HotKeysEventsHistory.Insert(0, eventArgs);
        }

        #endregion

        #region Handled Methods

        private void OnMouseAction(object sender, MouseHookEventArgs e)
        {
            MouseAction?.Invoke(this, e);

            if (e.Handled)
            {
                return;
            }

            if ((e.Action == Enums.MouseAction.LeftButtonPressed && GestureActionButton == MouseButtons.LeftMouseButton) || (e.Action == Enums.MouseAction.RightButtonPressed && GestureActionButton == MouseButtons.RightMouseButton))
            {
                // If left button is pressed, start work
                IsGestureMaking = true;
            }
            else if ((e.Action == Enums.MouseAction.LeftButtonReleased && GestureActionButton == MouseButtons.LeftMouseButton) || e.Action == Enums.MouseAction.RightButtonReleased && GestureActionButton == MouseButtons.RightMouseButton)
            {
                // If left button is released, stop work
                IsGestureMaking = false;

                if (_mousePointsList.Count > 2)
                {
                    // Get gesture that was doing
                    var gesture = DetectGesture();
                    if (!string.Equals(gesture.Name, "-1", StringComparison.Ordinal) && gesture.CheckPoinstArray != null)
                    {
                        var eventArgs = new MouseGestureEventArgs(gesture.Name, gesture.CheckPoinstArray, DateTime.Now.Ticks);

                        AddEventToHistory(eventArgs);

                        e.Handled = true;
                        Logger.Instance.Information($"Mouse gesture detected by {nameof(MouseAndKeyboardHookService)}.");
                        MouseGestureDetected?.Invoke(this, eventArgs);
                    }
                }

                _mousePointsList.Clear();
            }
            else if (e.Action == Enums.MouseAction.MouseMove)
            {
                // If the mouse is moved, add point to coords list
                if (IsGestureMaking)
                {
                    _mousePointsList.Add(e.Coords);
                }
            }
        }

        private void OnKeyboardAction(object sender, KeyboardHookEventArgs e)
        {
            KeyboardAction?.Invoke(this, e);

            // Add or remove key from clicked keys list
            switch (e.State)
            {
                case KeyState.Pressed:
                    _clickedKeys.RemoveAll(p => p.Item2 == e.Key);
                    _clickedKeys.Add(new Tuple<DateTime, Key>(DateTime.Now, e.Key));
                    break;

                case KeyState.Released:
                    _clickedKeys.RemoveAll(p => p.Item2 == e.Key);
                    break;
            }

            // Remove old key that has not been removed correctly.
            _clickedKeys.RemoveAll(p => (DateTime.Now - p.Item1).TotalSeconds > 3);

            // Init list of called hot keys
            var checkedHotKeys = DetectHotKeys();

            for (var i = 0; i < checkedHotKeys.Count; i++)
            {
                var eventArgs = new HotKeyEventArgs(checkedHotKeys[i].Name, checkedHotKeys[i].Keys, DateTime.Now.Ticks);

                AddEventToHistory(eventArgs);
                _clickedKeys.Clear();

                // Call OnHotKeyAction event with specific name and list of keys  
                Logger.Instance.Information($"Keyboard shortcut detected by {nameof(MouseAndKeyboardHookService)}.");
                HotKeyDetected?.Invoke(this, eventArgs);

                e.Handled = eventArgs.Handled;
            }
        }

        #endregion
    }
}
