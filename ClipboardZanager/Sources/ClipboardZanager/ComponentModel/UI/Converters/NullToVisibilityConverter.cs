using System;
using System.Windows;
using System.Windows.Data;

namespace ClipboardZanager.ComponentModel.UI.Converters
{
    /// <summary>
    /// Convert a null value or empty string to a <see cref="Visibility"/> value.
    /// </summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    internal class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets a value that defines whether the converter behavior is inverted or not
        /// </summary>
        public bool IsInverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                if (!IsInverted)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            else
            {
                if (!IsInverted)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
