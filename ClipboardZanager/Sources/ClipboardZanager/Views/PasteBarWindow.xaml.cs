using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ClipboardZanager.ComponentModel.Enums;
using ClipboardZanager.ComponentModel.Messages;
using ClipboardZanager.ComponentModel.UI.Controls;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Properties;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.ViewModels;
using GalaSoft.MvvmLight.Messaging;
using ClipboardZanager.ComponentModel.UI;

namespace ClipboardZanager.Views
{
    /// <summary>
    /// Interaction logic for PasteBarWindow.xaml
    /// </summary>
    public partial class PasteBarWindow : BlurredWindow
    {
        #region Fields

        private Storyboard _openingStoryboard;
        private Storyboard _closingStoryboard;
        private bool _isDisplayed;
        private Action _actionOnHidding;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="PasteBarWindow"/> class.
        /// </summary>
        public PasteBarWindow()
        {
            InitializeComponent();
            InitializePosition();

            if (Debugger.IsAttached)
            {
                Topmost = false;
            }
        }

        #endregion

        #region Handled Methods

        private void PasteBarWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Hide();
            Logger.Instance.Information("Paste bar loaded.");
        }

        private void ClosingStoryboard_Completed(object sender, EventArgs e)
        {
            Hide();
            _isDisplayed = false;
            _actionOnHidding();
            _closingStoryboard.Remove();
            _openingStoryboard.Remove();

            var dataContext = (PasteBarWindowViewModel)DataContext;
            if (dataContext.DataEntries.Any(item => item.IsMoreInfoDisplayed))
            {
                foreach (var dataContextDataEntry in dataContext.DataEntries.Where(item => item.IsMoreInfoDisplayed))
                {
                    dataContextDataEntry.IsMoreInfoDisplayed = false;
                }
                DataListBox.Items.Refresh();
            }

            if (DataListBox.Items.Count > 0)
            {
                DataListBox.ScrollIntoView(DataListBox.Items[0]);
            }

            DataListBox.SelectedItem = null;
            DataListBox.SelectedIndex = 0;
        }

        private void PasteBarWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Logger.Instance.Information("Paste bar closing.");
        }

        private void ContextMenu_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var dataContext = (PasteBarWindowViewModel)DataContext;
            dataContext.ContextMenuOpenedCommand.Execute(null);
        }

        private void ScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
        }

        #endregion

        #region Methods 

        /// <summary>
        /// Display on the screen the paste bare with an animation.
        /// </summary>
        /// <param name="actionOnHidding">Action to perform when the paste bar is hidden.</param>
        internal void DisplayBar(Action actionOnHidding)
        {
            if (_isDisplayed)
            {
                throw new Exception("Paste bar already displayed.");
            }

            var dataContext = (PasteBarWindowViewModel)DataContext;
            var delayer = new Delayer<object>(TimeSpan.FromMilliseconds(50));
            _isDisplayed = true;
            _actionOnHidding = actionOnHidding;

            InitializePosition();
            InitializeStoryboards();

            Messenger.Default.Register<ComponentModel.Messages.Message>(this, MessageIdentifiers.HidePasteBarWindow, HidePasteBarWindow);

            Show();
            Activate();
            Focus();
            var firstFocus = VisualHelper.FindVisualChildren<System.Windows.Controls.ListBox>(this).FirstOrDefault();
            firstFocus.Focus();
            Keyboard.Focus(firstFocus);

            Logger.Instance.Information("Paste bar displayed.");

            delayer.Action += (o, args) => _openingStoryboard.Begin();
            delayer.ResetAndTick();

            dataContext.DisplayBar();
        }

        /// <summary>
        /// Called when the mouse moves away from the paste bar window.
        /// </summary>
        /// <param name="message"></param>
        private void HidePasteBarWindow(ComponentModel.Messages.Message message)
        {
            Logger.Instance.Information("Paste bar is hiding.");
            Messenger.Default.Unregister<ComponentModel.Messages.Message>(this, MessageIdentifiers.HidePasteBarWindow, HidePasteBarWindow);
            _closingStoryboard.Begin();
        }

        /// <summary>
        /// Determines the size and location of the window on the screen before it is displayed.
        /// </summary>
        private void InitializePosition()
        {
            var activeScreen = Screen.FromPoint(new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
            var screen = SystemInfoHelper.GetAllScreenInfos().Single(s => s.DeviceName == activeScreen.DeviceName);
            var scale = screen.Scale / 100.0;

            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = screen.Bounds.Left;
            Width = screen.Bounds.Right / scale;
            Height = screen.Bounds.Bottom / 9 * 3 / scale;

            if (Height < MinHeight)
            {
                Height = MinHeight;
            }

            if (Settings.Default.PasteBarPosition == PasteBarPosition.Top)
            {
                Top = screen.Bounds.Top - Height;
            }
            else
            {
                Top = screen.Bounds.Bottom / scale;
            }

            Hide();
        }

        /// <summary>
        /// Initialize the opening and closing storyboard.
        /// </summary>
        private void InitializeStoryboards()
        {
            double openFrom;
            double openTo;
            double closeFrom;
            double closeTo;

            if (Settings.Default.PasteBarPosition == PasteBarPosition.Top)
            {
                openFrom = -Height;
                openTo = 0;
            }
            else
            {
                openFrom = Top;
                openTo = Top - Height;
            }

            closeFrom = openTo;
            closeTo = openFrom;

            var openDoubleAnimation = new DoubleAnimation();
            openDoubleAnimation.From = openFrom;
            openDoubleAnimation.To = openTo;
            openDoubleAnimation.Duration = TimeSpan.FromMilliseconds(100);
            openDoubleAnimation.EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut };
            _openingStoryboard = new Storyboard();
            _openingStoryboard.RepeatBehavior = new RepeatBehavior(1);
            Storyboard.SetTargetProperty(openDoubleAnimation, new PropertyPath(TopProperty));
            Storyboard.SetTarget(openDoubleAnimation, this);
            _openingStoryboard.Children.Add(openDoubleAnimation);

            var closeDoubleAnimation = new DoubleAnimation();
            closeDoubleAnimation.From = closeFrom;
            closeDoubleAnimation.To = closeTo;
            closeDoubleAnimation.Duration = TimeSpan.FromMilliseconds(100);
            closeDoubleAnimation.EasingFunction = new CircleEase { EasingMode = EasingMode.EaseIn };
            _closingStoryboard = new Storyboard();
            _closingStoryboard.RepeatBehavior = new RepeatBehavior(1);
            Storyboard.SetTargetProperty(closeDoubleAnimation, new PropertyPath(TopProperty));
            Storyboard.SetTarget(closeDoubleAnimation, this);
            _closingStoryboard.Children.Add(closeDoubleAnimation);
            _closingStoryboard.Completed += ClosingStoryboard_Completed;
        }

        #endregion
    }
}
