using System;
using System.Windows;
using System.Windows.Data;

namespace ClipboardZanager.ComponentModel.UI.Converters
{
    /// <summary>
    /// Convert an <see cref="Enum"/> to a <see cref="bool"/> value.
    /// </summary>
    [ValueConversion(typeof(Enum), typeof(bool))]
    internal class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var parameterString = parameter as string;
            if (parameterString == null)
            {
                return DependencyProperty.UnsetValue;
            }

            if (Enum.IsDefined(value.GetType(), value) == false)
            {
                return DependencyProperty.UnsetValue;
            }

            var parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var valueBool = value as bool?;

            if (!valueBool.HasValue)
            {
                return DependencyProperty.UnsetValue;
            }

            var parameterString = parameter as string;
            if (parameterString == null)
            {
                return DependencyProperty.UnsetValue;
            }

            if (!valueBool.Value)
            {
                return Enum.GetValues(targetType).GetValue(0);
            }

            return Enum.Parse(targetType, parameterString);
        }
    }
}
