using System;
using System.Globalization;
using System.Windows.Data;

namespace ClipboardZanager.ComponentModel.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="bool"/> to its inverted value
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    internal class BooleanToInvertedBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }

            throw new ArgumentException("Boolean value needed", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }

            throw new ArgumentException("Boolean value needed", nameof(value));
        }
    }
}
