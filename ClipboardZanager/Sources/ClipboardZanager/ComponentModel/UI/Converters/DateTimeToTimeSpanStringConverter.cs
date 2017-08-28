using System;
using System.Windows;
using System.Windows.Data;
using ClipboardZanager.Strings;

namespace ClipboardZanager.ComponentModel.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="DateTime"/> to a <see cref="string"/> that represents the time span.
    /// </summary>
    [ValueConversion(typeof(DateTime), typeof(string))]
    internal class DateTimeToTimeSpanStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var dateTimeValue = value as DateTime?;

            if (dateTimeValue == null)
            {
                return DependencyProperty.UnsetValue;
            }

            string ago;
            var time = string.Empty;
            var timeUnit = string.Empty;
            var dateTime = dateTimeValue.Value;
            var timeSpan = DateTime.Now.Subtract(dateTime);
            var language = LanguageManager.GetInstance().PasteBarWindow;

            if (timeSpan < TimeSpan.FromMinutes(2))
            {
                ago = language.TimeSpan_LessThanAMinute;
            }
            else if (timeSpan < TimeSpan.FromMinutes(60))
            {
                ago = language.TimeSpan_Ago;
                time = timeSpan.Minutes.ToString();
                timeUnit = language.TimeSpan_Minutes;
            }
            else if (timeSpan < TimeSpan.FromHours(2))
            {
                ago = language.TimeSpan_Ago;
                time = timeSpan.Hours.ToString();
                timeUnit = language.TimeSpan_Hour;
            }
            else if (timeSpan < TimeSpan.FromHours(24))
            {
                ago = language.TimeSpan_Ago;
                time = timeSpan.Hours.ToString();
                timeUnit = language.TimeSpan_Hours;
            }
            else if (timeSpan < TimeSpan.FromHours(48))
            {
                ago = language.TimeSpan_Yesterday;
            }
            else
            {
                ago = language.TimeSpan_Ago;
                time = timeSpan.Days.ToString();
                timeUnit = language.TimeSpan_Days;
            }

            return string.Format(language.TimeSpan, time, timeUnit, ago);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
