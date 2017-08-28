using System.Windows;
using System.Windows.Controls;
using ClipboardZanager.Shared.Core;

namespace ClipboardZanager.ComponentModel.UI.Controls
{
    /// <summary>
    /// Represents a control that indicates that an operation is ongoing. The typical visual appearance is a ring-shaped "spinner" that cycles an animation as progress continues.
    /// </summary>
    [TemplateVisualState(Name = "Inactive", GroupName = "ActiveStates")]
    [TemplateVisualState(Name = "Active", GroupName = "ActiveStates")]
    public class ProgressRing : Control
    {
        #region Properties

        /// <summary>
        /// Identifies the EllipseDiameterTemplateSetting dependency property.
        /// </summary>
        public static readonly DependencyProperty EllipseDiameterTemplateSettingProperty = DependencyProperty.Register("EllipseDiameterTemplateSetting", typeof(double), typeof(ProgressRing), new PropertyMetadata(default(double)));

        public double EllipseDiameterTemplateSetting
        {
            get { return (double)GetValue(EllipseDiameterTemplateSettingProperty); }
            private set { SetValue(EllipseDiameterTemplateSettingProperty, value); }
        }

        /// <summary>
        /// Identifies the EllipseOffsetTemplateSetting dependency property.
        /// </summary>
        public static readonly DependencyProperty EllipseOffsetTemplateSettingProperty = DependencyProperty.Register("EllipseOffsetTemplateSetting", typeof(Thickness), typeof(ProgressRing), new PropertyMetadata(default(Thickness)));

        public Thickness EllipseOffsetTemplateSetting
        {
            get { return (Thickness)GetValue(EllipseOffsetTemplateSettingProperty); }
            private set { SetValue(EllipseOffsetTemplateSettingProperty, value); }
        }

        /// <summary>
        /// Identifies the IsActive dependency property.
        /// </summary>
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(ProgressRing), new PropertyMetadata(false, IsActiveChanged));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        /// <summary>
        /// Identifies the MaxSideLengthTemplateSetting dependency property.
        /// </summary>
        public static readonly DependencyProperty MaxSideLengthTemplateSettingProperty = DependencyProperty.Register("MaxSideLengthTemplateSetting", typeof(double), typeof(ProgressRing), new PropertyMetadata(default(double)));

        public double MaxSideLengthTemplateSetting
        {
            get { return (double)GetValue(MaxSideLengthTemplateSettingProperty); }
            private set { SetValue(MaxSideLengthTemplateSettingProperty, value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new static instance of the <see cref="ProgressRing"/> class
        /// </summary>
        static ProgressRing()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressRing), new FrameworkPropertyMetadata(typeof(ProgressRing)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressRing"/> class.
        /// </summary>
        public ProgressRing()
        {
            SizeChanged += OnSizeChanged;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Is called when the template of the ProgressRing control is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateActiveState();
        }

        /// <summary>
        /// Is called when the IsActive dependency property is changed.
        /// </summary>
        /// <param name="d">The object that it's property is changed.</param>
        /// <param name="e">Event args of the property changed event.</param>
        private static void IsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressRing = d as ProgressRing;
            Requires.NotNull(progressRing, nameof(progressRing));
            progressRing.UpdateActiveState();
        }

        /// <summary>
        /// Is called when the size of the ProgressRing control is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateMaxSideLength();
            UpdateEllipseDiameter();
            UpdateEllipseOffset();
        }

        /// <summary>
        /// Update the visual state of a ProgressRing control.
        /// </summary>
        private void UpdateActiveState()
        {
            if (IsActive)
            {
                VisualStateManager.GoToState(this, "Active", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Inactive", true);
            }
        }

        /// <summary>
        /// Update the value of EllipseDiameter property.
        /// </summary>
        private void UpdateEllipseDiameter()
        {
            if (ActualWidth <= 25)
            {
                EllipseDiameterTemplateSetting = 3;
            }
            else
            {
                EllipseDiameterTemplateSetting = ActualWidth / 10 + 0.5;
            }
        }

        /// <summary>
        /// Update the value of EllipseOffset property.
        /// </summary>
        private void UpdateEllipseOffset()
        {
            if (ActualWidth <= 25)
            {
                EllipseOffsetTemplateSetting = new Thickness(0, 7, 0, 0);
            }
            else if (ActualWidth <= 30)
            {
                var top = ActualWidth * (9 / 20) - 9 / 2;
                EllipseOffsetTemplateSetting = new Thickness(0, top, 0, 0);
            }
            else
            {
                var top = ActualWidth * (2 / 5) - 2;
                EllipseOffsetTemplateSetting = new Thickness(0, top, 0, 0);
            }
        }

        /// <summary>
        /// Update the value of MaxSideLength property.
        /// </summary>
        private void UpdateMaxSideLength()
        {
            if (ActualWidth <= 25)
            {
                MaxSideLengthTemplateSetting = 20;
            }
            else
            {
                MaxSideLengthTemplateSetting = ActualWidth - 5;
            }
        }

        #endregion
    }
}
