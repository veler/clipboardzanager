using System;
using System.Windows;
using System.Windows.Data;

namespace ClipboardZanager.ComponentModel.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="FlowDirection"/> to a specified <see cref="string"/> value.
    /// </summary>
    [ValueConversion(typeof(FlowDirection), typeof(string))]
    public class FlowDirectionToStringConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets a value that will be use when the input is <see cref="FlowDirection.LeftToRight"/>
        /// </summary>
        public string LeftToRightValue { get; set; }

        /// <summary>
        /// Gets or sets a value that will be use when the input is <see cref="FlowDirection.RightToLeft"/>
        /// </summary>
        public string RightToLeftValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var valueBool = value as FlowDirection?;
            if (valueBool == null)
            {
                return DependencyProperty.UnsetValue;
            }

            if (valueBool.Value == FlowDirection.LeftToRight)
            {
                return LeftToRightValue;
            }
            else
            {
                return RightToLeftValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
