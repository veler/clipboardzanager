using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using ClipboardZanager.ComponentModel.Enums;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Hooking;
using ClipboardZanager.Core.Desktop.Interop;
using ClipboardZanager.Core.Desktop.Interop.Structs;
using ClipboardZanager.Core.Desktop.ComponentModel;
using Microsoft.Win32;

namespace ClipboardZanager.ComponentModel.UI.Controls
{
    /// <summary>
    /// Provides an alternative WindowBase which uses the AeroGlass effect of Windows 10.
    /// </summary>
    public class BlurredWindow : Window
    {
        #region Fields

        private readonly RegistryKey _personalizeRegistryKey;
        private bool _customBackground;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value that defines wether the blur effect must be applied or not. Please not that if the user disabled Blur under Windows, it wont be applied.
        /// </summary>
        public static readonly DependencyProperty IsBlurredProperty = DependencyProperty.Register("IsBlurred", typeof(bool), typeof(BlurredWindow), new PropertyMetadata(true, IsBlurredUseAccentColorPropertyChangedCallback));

        /// <summary>
        /// Gets or sets a value that defines wether the blur effect must be applied or not. Please not that if the user disabled Blur under Windows, it wont be applied.
        /// </summary>
        public bool IsBlurred
        {
            get { return (bool)GetValue(IsBlurredProperty); }
            set { SetValue(IsBlurredProperty, value); }
        }

        private static readonly DependencyPropertyKey IsBlurRenderedPropertyKey = DependencyProperty.RegisterReadOnly("IsBlurRendered", typeof(bool), typeof(BlurredWindow), new PropertyMetadata(false));

        /// <summary>
        /// Gets a value that defines wether the blur effect is actually applied.
        /// </summary>
        public static readonly DependencyProperty IsBlurRenderedProperty = IsBlurRenderedPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets a value that defines wether the blur effect is actually applied.
        /// </summary>
        public bool IsBlurRendered
        {
            get { return (bool)GetValue(IsBlurRenderedProperty); }
            private set { SetValue(IsBlurRenderedPropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets a value that defines wether the system accent color should be used as the window background brush or the default dark gray color.
        /// </summary>
        public static readonly DependencyProperty UseAccentColorProperty = DependencyProperty.Register("UseAccentColor", typeof(AccentColorUse), typeof(BlurredWindow), new PropertyMetadata(AccentColorUse.Yes, IsBlurredUseAccentColorPropertyChangedCallback));

        /// <summary>
        /// Gets or sets a value that defines wether the system accent color should be used as the window background brush or the default dark gray color.
        /// </summary>
        public AccentColorUse UseAccentColor
        {
            get { return (AccentColorUse)GetValue(UseAccentColorProperty); }
            set { SetValue(UseAccentColorProperty, value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize the static instance of the <see cref="BlurredWindow"/> class.
        /// </summary>
        static BlurredWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BlurredWindow), new FrameworkPropertyMetadata(typeof(BlurredWindow)));
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="BlurredWindow"/> class.
        /// </summary>
        public BlurredWindow()
        {
            lock(this)
            {
                _personalizeRegistryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            }

            Loaded += BlurredWindow_Loaded;
            Closed += BlurredWindow_Closed;
        }

        #endregion

        #region Handled Methods

        internal void SystemEvents_UserPreferenceChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var transparency = (int)_personalizeRegistryKey.GetValue("EnableTransparency", 1);

                if (!_customBackground)
                {
                    switch (UseAccentColor)
                    {
                        case AccentColorUse.No:
                            if (SystemParameters.HighContrast)
                            {
                                Background = GetWindowsAccentColor();
                            }
                            else if (transparency == 1 && IsBlurred)
                            {
                                Background = new SolidColorBrush(Color.FromArgb(153, 31, 31, 31));
                            }
                            else
                            {
                                Background = new SolidColorBrush(Color.FromArgb(255, 31, 31, 31));
                            }
                            break;

                        case AccentColorUse.Yes:
                            if (transparency == 1 && !SystemParameters.HighContrast && IsBlurred)
                            {
                                Background = ColorStrength(GetWindowsAccentColor());
                            }
                            else
                            {
                                Background = GetWindowsAccentColor();
                            }
                            break;

                        case AccentColorUse.Auto:
                            var useAccentColorOnTaskBarAndStarMenu = (int)_personalizeRegistryKey.GetValue("ColorPrevalence", 0);

                            if (useAccentColorOnTaskBarAndStarMenu == 1)
                            {
                                if (transparency == 1 && !SystemParameters.HighContrast && IsBlurred)
                                {
                                    Background = ColorStrength(GetWindowsAccentColor());
                                }
                                else
                                {
                                    Background = GetWindowsAccentColor();
                                }
                            }
                            else
                            {
                                if (SystemParameters.HighContrast)
                                {
                                    Background = GetWindowsAccentColor();
                                }
                                else if (transparency == 1 && IsBlurred)
                                {
                                    Background = new SolidColorBrush(Color.FromArgb(153, 31, 31, 31));
                                }
                                else
                                {
                                    Background = new SolidColorBrush(Color.FromArgb(255, 31, 31, 31));
                                }
                            }
                            break;
                    }
                }

                if (transparency == 1 && IsBlurred)
                {
                    EnableOrDisableBlur(true);
                    IsBlurRendered = true;
                }
                else
                {
                    EnableOrDisableBlur(false);
                    IsBlurRendered = false;
                }
            });
        }

        private void BlurredWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 31, 31, 31));
            }

            _customBackground = Background != null;

            if (!_customBackground)
            {
                SystemParameters.StaticPropertyChanged += SystemEvents_UserPreferenceChanged;
                SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            }

            SystemEvents_UserPreferenceChanged(sender, e);
        }

        private void BlurredWindow_Closed(object sender, EventArgs e)
        {
            SystemParameters.StaticPropertyChanged -= SystemEvents_UserPreferenceChanged;
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Enable or disable the blur effect on the window.
        /// </summary>
        /// <param name="enable">Defines wether the blur effect must be enabled or not.</param>
        private void EnableOrDisableBlur(bool enable)
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy
            {
                AccentState = enable ? AccentState.ACCENT_ENABLE_BLURBEHIND : AccentState.ACCENT_DISABLED
            };

            var accentStructSize = Marshal.SizeOf(accent);
            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            NativeMethods.SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        /// <summary>
        /// returns a <see cref="SolidColorBrush"/> of the given <see cref="SolidColorBrush"/> with chosen opacity
        /// </summary>
        /// <param name="colorBrush"></param>
        /// <param name="strength">opacity weight from 0 to 255 and is set to 191 if no value is provided</param>
        /// <returns>a given <see cref="SolidColorBrush"/> with chosen opacity</returns>
        private static SolidColorBrush ColorStrength(SolidColorBrush colorBrush, double strength = 0.75d)
        {
            if (strength < 0d || strength > 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(strength), @"strength must be a value between 0.0 and 1.0");
            }

            var color = colorBrush.Color;
            color.A = (byte)(strength * 255);
            return new SolidColorBrush(color);
        }

        /// <summary>
        /// Retrieves the Windows accent color.
        /// </summary>
        /// <returns>A <see cref="SolidColorBrush"/> that corresponds to the accent color.</returns>
        private SolidColorBrush GetWindowsAccentColor()
        {
            if (SystemParameters.HighContrast)
            {
                return new SolidColorBrush(Colors.Black);
            }

            return new SolidColorBrush(AccentColorSet.ActiveSet["SystemAccent"]);
        }

        #endregion

        #region Properties Changed Callback

        private static void IsBlurredUseAccentColorPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var window = (BlurredWindow)dependencyObject;

            if (window.IsLoaded)
            {
                window.SystemEvents_UserPreferenceChanged(window, null);
            }
        }

        #endregion
    }
}
