using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ClipboardZanager.ComponentModel.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="bool"/> to a specified <see cref="Brush"/> value.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Brush))]
    internal class BooleanToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets a value that will be use when the input is true
        /// </summary>
        public Brush TrueValue { get; set; }

        /// <summary>
        /// Gets or sets a value that will be use when the input is false
        /// </summary>
        public Brush FalseValue { get; set; }

        /// <summary>
        /// Gets or sets a value that will be use when the input is true
        /// </summary>
        public Brush HighContrastTrueValue { get; set; }

        /// <summary>
        /// Gets or sets a value that will be use when the input is false
        /// </summary>
        public Brush HighContrastFalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var valueBool = value as bool?;
            if (valueBool == null)
            {
                return DependencyProperty.UnsetValue;
            }

            if (SystemParameters.HighContrast)
            {
                if (valueBool.Value)
                {
                    return HighContrastTrueValue;
                }
                else
                {
                    return HighContrastFalseValue;
                }
            }
            else
            {
                if (valueBool.Value)
                {
                    return TrueValue;
                }
                else
                {
                    return FalseValue;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
