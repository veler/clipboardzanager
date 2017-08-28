using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClipboardZanager.ComponentModel.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="double"/> to a <see cref="Thickness"/> value.
    /// </summary>
    [ValueConversion(typeof(double), typeof(Thickness))]
    internal class WidthToPaneMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                var doubleValue = (double)value;
                return new Thickness(-doubleValue, 0, -doubleValue, 0);
            }
            throw new ArgumentException("Double value needed", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
