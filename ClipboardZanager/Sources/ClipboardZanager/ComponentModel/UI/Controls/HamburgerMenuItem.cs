using System.Windows;
using System.Windows.Controls;

namespace ClipboardZanager.ComponentModel.UI.Controls
{
    /// <summary>
    /// Represents an hamburger menu item
    /// </summary>
    public class HamburgerMenuItem : ListBoxItem
    {
        #region Properties

        /// <summary>
        /// The icon property.
        /// </summary>
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(HamburgerMenuItem), new UIPropertyMetadata());

        /// <summary>
        /// Gets or sets the icon of the item
        /// </summary>
        public object Icon
        {
            get { return GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new static instance of the <see cref="HamburgerMenuItem"/> class
        /// </summary>
        static HamburgerMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HamburgerMenuItem), new FrameworkPropertyMetadata(typeof(HamburgerMenuItem)));
        }

        #endregion
    }
}
