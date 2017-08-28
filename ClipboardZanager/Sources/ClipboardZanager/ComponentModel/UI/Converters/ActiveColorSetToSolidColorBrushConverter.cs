using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ClipboardZanager.Core.Desktop.Interop;

namespace ClipboardZanager.ComponentModel.UI.Converters
{
    /// <summary>
    /// Convert a system color name to a <see cref="SolidColorBrush"/> value.
    /// </summary>
    [ValueConversion(typeof(string), typeof(SolidColorBrush))]
    internal class ActiveColorSetToSolidColorBrushConverter : IValueConverter
    {
        public string ColorName { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (SystemParameters.HighContrast)
            {
                return new SolidColorBrush(Colors.White);
            }

            return new SolidColorBrush(AccentColorSet.ActiveSet[ColorName]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
