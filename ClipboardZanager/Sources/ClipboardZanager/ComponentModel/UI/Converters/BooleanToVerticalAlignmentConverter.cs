using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClipboardZanager.ComponentModel.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="bool"/> to a <see cref="VerticalAlignment"/> value.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(VerticalAlignment))]
    public class BooleanToVerticalAlignmentConverter : IValueConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanToVerticalAlignmentConverter"/> class.
        /// </summary>
        public BooleanToVerticalAlignmentConverter()
        {
            True = VerticalAlignment.Stretch;
            False = VerticalAlignment.Center;
        }

        /// <summary>
        /// Gets or sets the VerticalAlignment to return when the value is True.
        /// </summary>
        public VerticalAlignment True { get; set; }

        /// <summary>
        /// Gets or sets the VerticalAlignment to return when the value is False.
        /// </summary>
        public VerticalAlignment False { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value ? True : False;
            }

            throw new ArgumentException("Boolean value needed", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VerticalAlignment)
            {
                return (VerticalAlignment)value == True;
            }

            throw new ArgumentException("VerticalAlignment value needed", nameof(value));
        }
    }
}
