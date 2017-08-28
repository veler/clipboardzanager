using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClipboardZanager.ComponentModel.UI.Controls
{
    /// <summary>
    /// Represents a text box with the possibility to clear the content with a cross.
    /// </summary>
    public class TouchTextBox : TextBox
    {
        #region Fields

        private Viewbox _clearViewbox;
        private Grid _clearClickableZone;
        private TextBlock _placeHolderTextBlock;

        #endregion

        #region Properties

        /// <summary>
        /// The can clear property.
        /// </summary>
        public static readonly DependencyProperty CanClearProperty = DependencyProperty.Register("CanClear", typeof(bool), typeof(TouchTextBox), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value indicating whether can clear the text box.
        /// </summary>
        public bool CanClear
        {
            get { return (bool)GetValue(CanClearProperty); }
            set
            {
                SetValue(CanClearProperty, value);
                TouchTextBoxTextChanged(this, null);
            }
        }

        /// <summary>
        /// The place holder property.
        /// </summary>
        public static readonly DependencyProperty PlaceHolderProperty = DependencyProperty.Register("PlaceHolder", typeof(string), typeof(TouchTextBox), new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the water mark.
        /// </summary>
        public string PlaceHolder
        {
            get { return (string)GetValue(PlaceHolderProperty); }
            set { SetValue(PlaceHolderProperty, value); }
        }

        /// <summary>
        /// The place holder foreground property.
        /// </summary>
        public static readonly DependencyProperty PlaceHolderForegroundProperty = DependencyProperty.Register("PlaceHolderForeground", typeof(Brush), typeof(TouchTextBox), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the place holder foreground.
        /// </summary>
        public Brush PlaceHolderForeground
        {
            get { return (Brush)GetValue(PlaceHolderForegroundProperty); }
            set { SetValue(PlaceHolderForegroundProperty, value); }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize a new static instance of the <see cref="TouchTextBox"/> class
        /// </summary>
        static TouchTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TouchTextBox), new FrameworkPropertyMetadata(typeof(TouchTextBox)));
        }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _clearViewbox = (Viewbox)Template.FindName("ClearViewbox", this);
            _clearClickableZone = (Grid)Template.FindName("ClearClickableZone", this);
            _placeHolderTextBlock = (TextBlock)Template.FindName("PlaceHolderTextBlock", this);

            _clearClickableZone.MouseUp += ClearClickableZoneMouseUp;

            TextChanged += TouchTextBoxTextChanged;
            GotFocus += TouchTextBoxGotFocus;
            LostFocus += TouchTextBoxLostFocus;
            GotKeyboardFocus += TouchTextBoxGotKeyboardFocus;
            LostKeyboardFocus += TouchTextBoxLostKeyboardFocus;

            TouchTextBoxTextChanged(this, null);
        }

        #endregion

        #region Handled Methods

        /// <summary>
        /// The text box lost keyboard focus.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TouchTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TouchTextBoxLostFocus(sender, null);
        }

        /// <summary>
        /// The text box got keyboard focus.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TouchTextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TouchTextBoxGotFocus(sender, null);
        }

        /// <summary>
        /// The text box lost focus.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TouchTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            _clearViewbox.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(Text))
            {
                _placeHolderTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                _placeHolderTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// The text box got focus.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TouchTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            _placeHolderTextBlock.Visibility = Visibility.Collapsed;

            if (CanClear && !string.IsNullOrEmpty(Text) && !IsReadOnly && IsEnabled)
            {
                _clearViewbox.Visibility = Visibility.Visible;
            }
            else
            {
                _clearViewbox.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// The text box text changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TouchTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsFocused || IsKeyboardFocused)
            {
                TouchTextBoxGotFocus(sender, null);
            }
            else
            {
                TouchTextBoxLostFocus(sender, null);
            }
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

        #endregion
    }
}
