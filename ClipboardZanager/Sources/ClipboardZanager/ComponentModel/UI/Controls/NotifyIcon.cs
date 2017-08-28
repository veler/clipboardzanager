using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ClipboardZanager.ComponentModel.Enums;
using ClipboardZanager.Core.Desktop.Services;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Services;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;
using ClipboardZanager.Core.Desktop.Interop;
using System.Windows.Interop;
using System.Windows.Controls.Primitives;
using ClipboardZanager.Core.Desktop.ComponentModel;
using System.Linq;

namespace ClipboardZanager.ComponentModel.UI.Controls
{
    /// <summary>
    /// Represents a Notify Icon in the Windows task bar.
    /// </summary>
    [ContentProperty("Text")]
    public class NotifyIcon : FrameworkElement
    {
        #region Fields

        private Forms.NotifyIcon _notifyIcon;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the icon of the balloon notification.
        /// </summary>
        public static readonly DependencyProperty BalloonTipIconProperty = DependencyProperty.Register("BalloonTipIcon", typeof(BalloonTipIcon), typeof(NotifyIcon));

        /// <summary>
        /// Gets or sets the icon of the balloon notification.
        /// </summary>
        public BalloonTipIcon BalloonTipIcon
        {
            get { return (BalloonTipIcon)GetValue(BalloonTipIconProperty); }
            set { SetValue(BalloonTipIconProperty, value); }
        }

        /// <summary>
        /// Gets or sets the text to display into the balloon notification.
        /// </summary>
        public static readonly DependencyProperty BalloonTipTextProperty = DependencyProperty.Register("BalloonTipText", typeof(string), typeof(NotifyIcon));

        /// <summary>
        /// Gets or sets the text to display into the balloon notification.
        /// </summary>
        public string BalloonTipText
        {
            get { return (string)GetValue(BalloonTipTextProperty); }
            set { SetValue(BalloonTipTextProperty, value); }
        }

        /// <summary>
        /// Gets or sets the title to display into the balloon notification.
        /// </summary>
        public static readonly DependencyProperty BalloonTipTitleProperty = DependencyProperty.Register("BalloonTipTitle", typeof(string), typeof(NotifyIcon));

        /// <summary>
        /// Gets or sets the title to display into the balloon notification.
        /// </summary>
        public string BalloonTipTitle
        {
            get { return (string)GetValue(BalloonTipTitleProperty); }
            set { SetValue(BalloonTipTitleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the icon of the <see cref="NotifyIcon"/>
        /// </summary>
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(NotifyIcon), new FrameworkPropertyMetadata(OnIconChanged));

        /// <summary>
        /// Gets or sets the icon of the <see cref="NotifyIcon"/>
        /// </summary>
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        /// <summary>
        /// Gets or sets the text of the <see cref="NotifyIcon"/>
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(NotifyIcon), new PropertyMetadata(OnTextChanged));

        /// <summary>
        /// Gets or sets the text of the <see cref="NotifyIcon"/>
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Gets or sets the text of the <see cref="NotifyIcon"/>
        /// </summary>
        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register("IconVisibility", typeof(Visibility), typeof(NotifyIcon), new PropertyMetadata(OnVisibilityChanged));

        /// <summary>
        /// Gets or sets the text of the <see cref="NotifyIcon"/>
        /// </summary>
        public Visibility IconVisibility
        {
            get { return (Visibility)GetValue(IconVisibilityProperty); }
            set { SetValue(IconVisibilityProperty, value); }
        }

        #endregion

        #region Events

        internal static readonly RoutedEvent MouseClickEvent = EventManager.RegisterRoutedEvent("MouseClick", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(NotifyIcon));

        /// <summary>
        /// Raised when we click on the notify icon in the task bar.
        /// </summary>
        internal event MouseButtonEventHandler MouseClick
        {
            add { AddHandler(MouseClickEvent, value); }
            remove { RemoveHandler(MouseClickEvent, value); }
        }

        internal static readonly RoutedEvent MouseDoubleClickEvent = EventManager.RegisterRoutedEvent("MouseDoubleClick", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(NotifyIcon));

        /// <summary>
        /// Raised when we double click on the notify icon in the task bar.
        /// </summary>
        internal event MouseButtonEventHandler MouseDoubleClick
        {
            add { AddHandler(MouseDoubleClickEvent, value); }
            remove { RemoveHandler(MouseDoubleClickEvent, value); }
        }

        internal static readonly RoutedEvent ContextMenuClosedEvent = EventManager.RegisterRoutedEvent("ContextMenuClosed", RoutingStrategy.Bubble, typeof(EventHandler), typeof(NotifyIcon));

        /// <summary>
        /// Raised when the context menu is closed.
        /// </summary>
        internal event EventHandler ContextMenuClosed
        {
            add { AddHandler(ContextMenuClosedEvent, value); }
            remove { RemoveHandler(ContextMenuClosedEvent, value); }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void BeginInit()
        {
            base.BeginInit();

            _notifyIcon = new Forms.NotifyIcon();
            _notifyIcon.Text = Text;
            _notifyIcon.Icon = FromImageSource(Icon);
            _notifyIcon.Visible = Visibility == Visibility.Visible;

            _notifyIcon.MouseDown += OnMouseDown;
            _notifyIcon.MouseUp += OnMouseUp;
            _notifyIcon.MouseClick += OnMouseClick;
            _notifyIcon.MouseDoubleClick += OnMouseDoubleClick;
        }

        /// <inheritdoc/>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            AttachToWindowClose();
        }

        /// <summary>
        /// Display the balloon notification
        /// </summary>
        /// <param name="timeout">The time that the balloon must stay visible</param>
        internal void ShowBalloonTip(int timeout)
        {
            _notifyIcon.BalloonTipTitle = BalloonTipTitle;
            _notifyIcon.BalloonTipText = BalloonTipText;
            _notifyIcon.BalloonTipIcon = (Forms.ToolTipIcon)BalloonTipIcon;
            _notifyIcon.ShowBalloonTip(timeout);
        }

        /// <summary>
        /// Display the balloon notification
        /// </summary>
        /// <param name="timeout">The time that the balloon must stay visible</param>
        /// <param name="tipTitle">The title of the balloon</param>
        /// <param name="tipText">The text of the balloon</param>
        /// <param name="tipIcon">The icon of the balloon</param>
        internal void ShowBalloonTip(int timeout, string tipTitle, string tipText, BalloonTipIcon tipIcon)
        {
            _notifyIcon.ShowBalloonTip(timeout, tipTitle, tipText, (Forms.ToolTipIcon)tipIcon);
        }

        /// <summary>
        /// Show the contextual menu
        /// </summary>
        private void ShowContextMenu()
        {
            if (ContextMenu != null)
            {
                var mousePosition = new Core.Desktop.Interop.Structs.Point();
                NativeMethods.GetCursorPos(ref mousePosition);
                var activeScreen = Forms.Screen.FromPoint(new Drawing.Point(mousePosition.X, mousePosition.Y));
                var screen = SystemInfoHelper.GetAllScreenInfos().Single(s => s.DeviceName == activeScreen.DeviceName);

                ContextMenu.Placement = PlacementMode.Absolute;
                ContextMenu.HorizontalOffset = (mousePosition.X / (screen.Scale / 100.0)) - 2;
                ContextMenu.VerticalOffset = (mousePosition.Y / (screen.Scale / 100.0)) - 2;

                ContextMenu.Opened += OnContextMenuOpened;
                ContextMenu.Closed += OnContextMenuClosed;
                ContextMenu.IsOpen = true;

                HwndSource source = (HwndSource)PresentationSource.FromVisual(ContextMenu);
                if (source != null && source.Handle != IntPtr.Zero)
                {
                    //activate the context menu or the message window to track deactivation - otherwise, the context menu
                    //does not close if the user clicks somewhere else. With the message window
                    //fallback, the context menu can't receive keyboard events - should not happen though
                    NativeMethods.SetForegroundWindow(source.Handle);
                }
            }
        }

        private void AttachToWindowClose()
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Closed += (s, a) => _notifyIcon.Dispose();
            }
        }

        private static Drawing.Icon FromImageSource(ImageSource icon)
        {
            if (icon == null)
            {
                return null;
            }

            var iconUri = new Uri(icon.ToString());
            var resourceStream = Application.GetResourceStream(iconUri);

            Requires.NotNull(resourceStream, nameof(resourceStream));

            return new Drawing.Icon(resourceStream.Stream);
        }

        private static MouseButtonEventArgs CreateMouseButtonEventArgs(RoutedEvent handler, Forms.MouseButtons button)
        {
            return new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, ToMouseButton(button))
            {
                RoutedEvent = handler
            };
        }

        private static MouseButton ToMouseButton(Forms.MouseButtons button)
        {
            switch (button)
            {
                case Forms.MouseButtons.Left:
                    return MouseButton.Left;

                case Forms.MouseButtons.Right:
                    return MouseButton.Right;

                case Forms.MouseButtons.Middle:
                    return MouseButton.Middle;

                case Forms.MouseButtons.XButton1:
                    return MouseButton.XButton1;

                case Forms.MouseButtons.XButton2:
                    return MouseButton.XButton2;
            }

            throw new InvalidOperationException();
        }

        private static Rect GetContextMenuRect(ContextMenu menu)
        {
            if (PresentationSource.FromVisual(menu) == null)
            {
                return new Rect(0, 0, 0, 0);
            }

            var begin = menu.PointToScreen(new Point(0, 0));
            var end = menu.PointToScreen(new Point(menu.ActualWidth, menu.ActualHeight));
            return new Rect(begin, end);
        }

        #endregion

        #region Handled Methods

        private void OnMouseDown(object sender, Forms.MouseEventArgs e)
        {
            RaiseEvent(CreateMouseButtonEventArgs(MouseDownEvent, e.Button));
        }

        private void OnMouseUp(object sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Right)
            {
                ShowContextMenu();
            }

            RaiseEvent(CreateMouseButtonEventArgs(MouseUpEvent, e.Button));
        }

        private void OnMouseDoubleClick(object sender, Forms.MouseEventArgs e)
        {
            RaiseEvent(CreateMouseButtonEventArgs(MouseDoubleClickEvent, e.Button));
        }

        private void OnMouseClick(object sender, Forms.MouseEventArgs e)
        {
            RaiseEvent(CreateMouseButtonEventArgs(MouseClickEvent, e.Button));
        }

        private void OnContextMenuClosed(object sender, RoutedEventArgs e)
        {
            ServiceLocator.GetService<MouseAndKeyboardHookService>().MouseAction -= NotifyIcon_MouseAction;

            ContextMenu.Opened -= OnContextMenuOpened;
            ContextMenu.Closed -= OnContextMenuClosed;

            RaiseEvent(new RoutedEventArgs(ContextMenuClosedEvent));
        }

        private void OnContextMenuOpened(object sender, RoutedEventArgs e)
        {
            ServiceLocator.GetService<MouseAndKeyboardHookService>().MouseAction += NotifyIcon_MouseAction;
        }

        private void NotifyIcon_MouseAction(object sender, Core.Desktop.Events.MouseHookEventArgs e)
        {
            if (e.Action != Core.Desktop.Enums.MouseAction.LeftButtonPressed &&
                e.Action != Core.Desktop.Enums.MouseAction.RightButtonPressed)
            {
                return;
            }

            Core.Desktop.ComponentModel.DispatcherHelper.ThrowIfNotStaThread();

            var contextMenuRect = GetContextMenuRect(ContextMenu);
            var hitPoint = new Point(e.Coords.X, e.Coords.Y);

            if (!contextMenuRect.Contains(hitPoint))
            {
                ContextMenu.IsOpen = false;
            }
        }

        #endregion

        #region Properties Changed Callback

        private static void OnVisibilityChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var control = (NotifyIcon)target;
            control._notifyIcon.Visible = control.IconVisibility == Visibility.Visible;
        }

        private static void OnIconChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(target))
            {
                var control = (NotifyIcon)target;
                control._notifyIcon.Icon = FromImageSource(control.Icon);
            }
        }

        private static void OnTextChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var control = (NotifyIcon)target;
            control._notifyIcon.Text = control.Text;
        }

        #endregion
    }
}
