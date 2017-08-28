using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ClipboardZanager.Strings;
using GalaSoft.MvvmLight.Command;

namespace ClipboardZanager.ComponentModel.UI.Controls
{
    /// <summary>
    /// Represents an items control that displays one item at a time, and enables "flip" behavior for traversing its collection of items.
    /// </summary>
    public class FlipView : Selector
    {
        #region Fields

        private double _fromValue;
        private double _elasticFactor = 1.0;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the previous content.
        /// </summary>
        private ContentControl PreviousContent { get; set; }

        /// <summary>
        /// Gets or sets the current content.
        /// </summary>
        private ContentControl CurrentContent { get; set; }

        /// <summary>
        /// Gets or sets the next content.
        /// </summary>
        private ContentControl NextContent { get; set; }

        /// <summary>
        /// Gets or sets the content grid.
        /// </summary>
        private Grid ContentGrid { get; set; }

        /// <summary>
        /// Gets or sets the container grid.
        /// </summary>
        private Grid ContainerGrid { get; set; }

        /// <summary>
        /// Gets the <see cref="LanguageManager"/>
        /// </summary>
        private static readonly DependencyProperty LanguageProperty = DependencyProperty.Register("Language", typeof(LanguageManager), typeof(FlipView), new FrameworkPropertyMetadata(LanguageManager.GetInstance()));

        /// <summary>
        /// The can go next property.
        /// </summary>
        private static readonly DependencyProperty CanGoNextProperty = DependencyProperty.Register("CanGoNext", typeof(bool), typeof(FlipView), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value indicating whether the user can go next.
        /// </summary>
        private bool CanGoNext
        {
            get { return (bool)this.GetValue(CanGoNextProperty); }
            set { this.SetValue(CanGoNextProperty, value); }
        }

        /// <summary>
        /// The can go back property.
        /// </summary>
        private static readonly DependencyProperty CanGoBackProperty = DependencyProperty.Register("CanGoBack", typeof(bool), typeof(FlipView), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value indicating whether the user can go back.
        /// </summary>
        private bool CanGoBack
        {
            get { return (bool)this.GetValue(CanGoBackProperty); }
            set { this.SetValue(CanGoBackProperty, value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new static instance of the <see cref="FlipView"/> class
        /// </summary>
        static FlipView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlipView), new FrameworkPropertyMetadata(typeof(FlipView)));
            SelectedIndexProperty.OverrideMetadata(typeof(FlipView), new FrameworkPropertyMetadata(-1, OnSelectedIndexChanged));
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="FlipView"/> class
        /// </summary>
        public FlipView()
        {
            SelectedIndex = -1;
            InitializeCommands();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Initialize the commands of the View Model
        /// </summary>
        private void InitializeCommands()
        {
            PreviousCommand = new RelayCommand(ExecutePreviousCommand);
            NextCommand = new RelayCommand(ExecuteNextCommand);
        }

        #region Previous

        /// <summary>
        /// The previous command.
        /// </summary>
        public RelayCommand PreviousCommand { get; private set; }

        private void ExecutePreviousCommand()
        {
            if (CanGoBack)
            {
                SelectedIndex--;
            }
        }

        #endregion

        #region Next

        /// <summary>
        /// The next command.
        /// </summary>
        public RelayCommand NextCommand { get; private set; }

        private void ExecuteNextCommand()
        {
            if (CanGoNext)
            {
                SelectedIndex++;
            }
        }

        #endregion

        #endregion

        #region Overrides

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PreviousContent = GetTemplateChild("PreviousContent") as ContentControl;
            CurrentContent = GetTemplateChild("CurrentContent") as ContentControl;
            NextContent = GetTemplateChild("NextContent") as ContentControl;
            ContentGrid = GetTemplateChild("ContentGrid") as Grid;
            ContainerGrid = GetTemplateChild("ContainerGrid") as Grid;

            Loaded += FlipViewLoaded;
            ContentGrid.ManipulationStarting += ContentGridManipulationStarting;
            ContentGrid.ManipulationDelta += ContentGridManipulationDelta;
            ContentGrid.ManipulationCompleted += ContentGridManipulationCompleted;
        }

        /// <inheritdoc/>
        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            RefreshDisposition();
        }

        #endregion

        #region Handled Methods

        private void FlipViewLoaded(object sender, RoutedEventArgs e)
        {
            RefreshDisposition();
        }

        private void ContentGridManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = ContainerGrid;
            e.Handled = true;
        }

        private void ContentGridManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (!(ContentGrid.RenderTransform is MatrixTransform))
            {
                ContentGrid.RenderTransform = new MatrixTransform();
            }

            var doTranslation = true;
            var matrix = ((MatrixTransform)ContentGrid.RenderTransform).Matrix;
            var delta = e.DeltaManipulation;

            if (SelectedIndex == 0 && delta.Translation.X > 0 && _elasticFactor > 0 || SelectedIndex == Items.Count - 1 && delta.Translation.X < 0 && _elasticFactor > 0)
            {
                var totalOffsetX = e.CumulativeManipulation.Translation.X;
                if (Math.Abs(totalOffsetX) < ActualWidth / 4)
                {
                    _elasticFactor -= 0.05;
                }
                else
                {
                    doTranslation = false;
                }
            }

            if (doTranslation)
            {
                matrix.Translate(delta.Translation.X * _elasticFactor, 0);
                ContentGrid.RenderTransform = new MatrixTransform(matrix);
            }

            e.Handled = true;
        }

        private void ContentGridManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            _fromValue = e.TotalManipulation.Translation.X;
            if (Math.Abs(_fromValue) > ActualWidth / 4)
            {
                if (_fromValue > 0)
                {
                    if (SelectedIndex > 0)
                    {
                        SelectedIndex--;
                    }
                }
                else
                {
                    if (SelectedIndex < Items.Count - 1)
                    {
                        SelectedIndex++;
                    }
                }
            }
            else
            {
                _elasticFactor -= 0.1;
            }

            if (_elasticFactor < 1)
            {
                var translateTransform = ContentGrid.RenderTransform as TranslateTransform;
                if (translateTransform != null)
                {
                    RunSlideAnimation(0, translateTransform.X);
                }
                else
                {
                    RunSlideAnimation(0, ((MatrixTransform)ContentGrid.RenderTransform).Matrix.OffsetX);
                }
            }

            _elasticFactor = 1.0;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute the animation when the selected item changed.
        /// </summary>
        /// <param name="dependencyObject">The <see cref="DependencyObject"/></param>
        /// <param name="args">The <see cref="DependencyPropertyChangedEventArgs"/></param>
        private static void OnSelectedIndexChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var control = (FlipView)dependencyObject;

            control.OnSelectedIndexChanged(args);
        }

        /// <summary>
        /// Execute the animation when the selected item changed.
        /// </summary>
        /// <param name="args">The <see cref="DependencyPropertyChangedEventArgs"/></param>
        private void OnSelectedIndexChanged(DependencyPropertyChangedEventArgs args)
        {
            CanGoBack = SelectedIndex > 0;
            CanGoNext = SelectedIndex < Items.Count - 1;

            if (!EnsureTemplateParts())
            {
                return;
            }

            if ((int)args.NewValue < 0 || (int)args.NewValue >= Items.Count)
            {
                return;
            }

            double toValue;
            if ((int)args.OldValue < (int)args.NewValue)
            {
                toValue = -ActualWidth;
            }
            else
            {
                toValue = ActualWidth;
            }

            RunSlideAnimation(toValue, _fromValue);
        }

        /// <summary>
        /// Change the displayed content in the <see cref="FlipView"/>.
        /// </summary>
        private void RefreshDisposition()
        {
            if (!EnsureTemplateParts())
            {
                return;
            }

            Canvas.SetLeft(PreviousContent, -ActualWidth);
            Canvas.SetLeft(NextContent, ActualWidth);
            ContentGrid.RenderTransform = new TranslateTransform();

            PreviousContent.Content = GetItem(SelectedIndex - 1);
            CurrentContent.Content = GetItem(SelectedIndex);
            NextContent.Content = GetItem(SelectedIndex + 1);
            CommandManager.InvalidateRequerySuggested();

            VisualHelper.InitializeFocus(CurrentContent.Content as UIElement);
        }

        /// <summary>
        /// Generate and run a storyboard that perform an horizontal slide animation.
        /// </summary>
        /// <param name="toValue">The position to get.</param>
        /// <param name="fromValue">The position from where we start the animation.</param>
        private void RunSlideAnimation(double toValue, double fromValue)
        {
            if (!(ContentGrid.RenderTransform is TranslateTransform))
            {
                ContentGrid.RenderTransform = new TranslateTransform();
            }

            var story = GenerateStoryboard(ContentGrid, toValue, fromValue);
            story.Completed += (s, e) =>
            {
                RefreshDisposition();
            };
            story.Begin();
        }

        /// <summary>
        /// Check whether the template has been initialized.
        /// </summary>
        /// <returns>Returns true if the template has been initialized.</returns>
        private bool EnsureTemplateParts()
        {
            return CurrentContent != null && NextContent != null && PreviousContent != null && ContentGrid != null;
        }

        /// <summary>
        /// Gets the item at the specific position.
        /// </summary>
        /// <param name="index">The index of the item to get.</param>
        /// <returns>Returns the item at the specific position. If the index is out or range, a null value is returned.</returns>
        private object GetItem(int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                return null;
            }

            return Items[index];
        }

        /// <summary>
        /// Generates a <see cref="Storyboard"/> that moves an item from an horizontal position to another.
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/> to move.</param>
        /// <param name="to">The position to get.</param>
        /// <param name="from">The position from where we start the <see cref="Storyboard"/>.</param>
        /// <returns>A <see cref="Storyboard"/></returns>
        private Storyboard GenerateStoryboard(DependencyObject target, double to, double from)
        {
            var story = new Storyboard();
            Storyboard.SetTargetProperty(story, new PropertyPath("(TextBlock.RenderTransform).(TranslateTransform.X)"));
            Storyboard.SetTarget(story, target);

            var doubleAnimation = new DoubleAnimationUsingKeyFrames();

            var fromFrame = new EasingDoubleKeyFrame(from)
            {
                EasingFunction = new ExponentialEase
                {
                    EasingMode = EasingMode.EaseOut
                },
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))
            };

            var toFrame = new EasingDoubleKeyFrame(to)
            {
                EasingFunction = new ExponentialEase
                {
                    EasingMode = EasingMode.EaseOut
                },
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))
            };

            doubleAnimation.KeyFrames.Add(fromFrame);
            doubleAnimation.KeyFrames.Add(toFrame);
            story.Children.Add(doubleAnimation);

            return story;
        }

        #endregion
    }
}
