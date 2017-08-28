using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Events;

namespace ClipboardZanager.ComponentModel.UI.Controls
{
    /// <summary>
    /// Represents a text box designed to perform a research in the <see cref="PasteUserControl"/>.
    /// </summary>
    public class SearchTextBox : TextBox
    {
        #region Fields

        private Viewbox _clearViewbox;
        private Grid _clearClickableZone;
        private Viewbox _searchViewbox;
        private Grid _searchClickableZone;
        private Delayer<object> _delayedTextChangedTimer;

        #endregion

        #region Properties

        /// <summary>
        /// The can clear property.
        /// </summary>
        public static readonly DependencyProperty DelayedTextChangedTimeoutProperty = DependencyProperty.Register("DelayedTextChangedTimeout", typeof(int), typeof(SearchTextBox), new FrameworkPropertyMetadata(1000));

        /// <summary>
        /// Gets or sets a value indicating whether can clear the text box.
        /// </summary>
        public int DelayedTextChangedTimeout
        {
            get { return (int)GetValue(DelayedTextChangedTimeoutProperty); }
            set { SetValue(DelayedTextChangedTimeoutProperty, value); }
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when the text has changed and that a delay passed.
        /// </summary>
        public event RoutedEventHandler DelayedTextChanged
        {
            add { AddHandler(DelayedTextChangedEvent, value); }
            remove { RemoveHandler(DelayedTextChangedEvent, value); }
        }

        public static readonly RoutedEvent DelayedTextChangedEvent = EventManager.RegisterRoutedEvent("DelayedTextChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SearchTextBox));

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize a new static instance of the <see cref="SearchTextBox"/> class
        /// </summary>
        static SearchTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchTextBox), new FrameworkPropertyMetadata(typeof(SearchTextBox)));
        }

        #endregion

        #region Overrides

        /// <summary>
        /// The on apply template.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _clearViewbox = (Viewbox)Template.FindName("ClearViewbox", this);
            _clearClickableZone = (Grid)Template.FindName("ClearClickableZone", this);
            _searchViewbox = (Viewbox)Template.FindName("SearchViewbox", this);
            _searchClickableZone = (Grid)Template.FindName("SearchClickableZone", this);

            _clearClickableZone.MouseUp += ClearClickableZoneMouseUp;

            TextChanged += SearchTextBoxTextChanged;
            GotFocus += SearchTextBoxGotFocus;
            LostFocus += SearchTextBoxLostFocus;
            GotKeyboardFocus += SearchTextBoxGotKeyboardFocus;
            LostKeyboardFocus += SearchTextBoxLostKeyboardFocus;

            SearchTextBoxLostFocus(this, null);
        }

        #endregion

        #region Handled Methods

        /// <summary>
        /// The text box lost keyboard focus.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SearchTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            SearchTextBoxLostFocus(sender, null);
        }

        /// <summary>
        /// The text box got keyboard focus.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SearchTextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            SearchTextBoxGotFocus(sender, null);
        }

        /// <summary>
        /// The text box lost focus.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SearchTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Text))
            {
                _clearViewbox.Visibility = Visibility.Collapsed;
                _searchViewbox.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// The text box got focus.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SearchTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (!IsReadOnly && IsEnabled)
            {
                _searchViewbox.Visibility = Visibility.Collapsed;
                if (string.IsNullOrEmpty(Text))
                {
                    _clearViewbox.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _clearViewbox.Visibility = Visibility.Visible;
                }
            }
            else
            {
                _clearViewbox.Visibility = Visibility.Collapsed;
                _searchViewbox.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// The text box text changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SearchTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsFocused || IsKeyboardFocused)
            {
                SearchTextBoxGotFocus(sender, null);
            }
            else
            {
                SearchTextBoxLostFocus(sender, null);
            }

            InitializeDelayedTextChangedTimer();
        }

        /// <summary>
        /// The clear clickable zone mouse up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ClearClickableZoneMouseUp(object sender, MouseButtonEventArgs e)
        {
            Text = null;
        }

        private void DelayedTextChangedTimer_Tick(object sender, DelayerActionEventArgs<object> e)
        {
            RaiseEvent(new RoutedEventArgs(DelayedTextChangedEvent, this));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the <see cref="_delayedTextChangedTimer"/> each time that the text change
        /// </summary>
        private void InitializeDelayedTextChangedTimer()
        {
            _delayedTextChangedTimer?.Stop();

            if (_delayedTextChangedTimer == null)
            {
                _delayedTextChangedTimer = new Delayer<object>(TimeSpan.FromMilliseconds(DelayedTextChangedTimeout));
                _delayedTextChangedTimer.Action += DelayedTextChangedTimer_Tick;
            }

            _delayedTextChangedTimer.ResetAndTick();
        }

        #endregion
    }
}
