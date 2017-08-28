using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ClipboardZanager.ComponentModel.UI.Controls
{
    /// <summary>
    /// Represents a custom <see cref="ToggleButton"/>
    /// </summary>
    public class PasteBarToggleButton : ToggleButton
    {
        #region Properties

        /// <summary>
        /// The secondary foreground property.
        /// </summary>
        public static readonly DependencyProperty SecondaryForegroundProperty = DependencyProperty.Register("SecondaryForeground", typeof(Brush), typeof(PasteBarToggleButton), new UIPropertyMetadata());

        /// <summary>
        /// Gets or sets the secondary foreground used when the control is checked
        /// </summary>
        public Brush SecondaryForeground
        {
            get { return (Brush)GetValue(SecondaryForegroundProperty); }
            set { SetValue(SecondaryForegroundProperty, value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new static instance of the <see cref="PasteBarToggleButton"/> class
        /// </summary>
        static PasteBarToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PasteBarToggleButton), new FrameworkPropertyMetadata(typeof(PasteBarToggleButton)));
        }

        #endregion
    }
}
