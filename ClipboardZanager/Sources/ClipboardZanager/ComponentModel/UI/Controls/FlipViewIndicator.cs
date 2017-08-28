using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Command;

namespace ClipboardZanager.ComponentModel.UI.Controls
{
    public class FlipViewIndicator : Control
    {
        #region Properties

        /// <summary>
        /// The linked <see cref="FlipView"/>
        /// </summary>
        public static readonly DependencyProperty FlipViewProperty = DependencyProperty.Register("FlipView", typeof(FlipView), typeof(FlipViewIndicator), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the linked <see cref="FlipView"/>.
        /// </summary>
        public FlipView FlipView
        {
            get { return (FlipView)GetValue(FlipViewProperty); }
            set { SetValue(FlipViewProperty, value); }
        }

        /// <summary>
        /// A value that indicates whether the user can select and item with the <see cref="FlipViewIndicator"/>.
        /// </summary>
        public static readonly DependencyProperty IsInteractionEnabledProperty = DependencyProperty.Register("IsInteractionEnabled", typeof(bool), typeof(FlipViewIndicator), new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether the user can select and item with the <see cref="FlipViewIndicator"/>.
        /// </summary>
        public bool IsInteractionEnabled
        {
            get { return (bool)GetValue(IsInteractionEnabledProperty); }
            set { SetValue(IsInteractionEnabledProperty, value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new static instance of the <see cref="FlipViewIndicator"/> class
        /// </summary>
        static FlipViewIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlipViewIndicator), new FrameworkPropertyMetadata(typeof(FlipViewIndicator)));
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="FlipViewIndicator"/> class
        /// </summary>
        public FlipViewIndicator()
        {
        }

        #endregion
    }
}
