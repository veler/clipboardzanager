using System;
using System.Globalization;
using System.Windows.Data;
using ClipboardZanager.ComponentModel.Enums;

namespace ClipboardZanager.ComponentModel.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="int"/> to a new <see cref="int"/>.
    /// </summary>
    [ValueConversion(typeof(int), typeof(int))]
    internal class IntegerManipulationConverter : IValueConverter
    {
        public IntegerManipulation Manipulation { get; set; }

        public int Value { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double doub;
            if (!double.TryParse(value.ToString(), out doub))
            {
                throw new ArgumentException("The value must be an integer", nameof(value));
            }

            var integer = (int)doub;

            switch (Manipulation)
            {
                case IntegerManipulation.Addition:
                    return integer + Value;

                case IntegerManipulation.Substraction:
                    return integer - Value;

                case IntegerManipulation.Division:
                    return integer / Value;

                case IntegerManipulation.Multiplication:
                    return integer * Value;
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
