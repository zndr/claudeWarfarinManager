using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WarfarinManager.UI.Converters
{
    /// <summary>
    /// Converter da bool a Thickness per spessore bordo
    /// </summary>
    public class BoolToThicknessConverter : IValueConverter
    {
        /// <summary>
        /// Valore quando true (default: 2)
        /// </summary>
        public double TrueValue { get; set; } = 2.0;

        /// <summary>
        /// Valore quando false (default: 1)
        /// </summary>
        public double FalseValue { get; set; } = 1.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                return new Thickness(TrueValue);
            }

            return new Thickness(FalseValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
