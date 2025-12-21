using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WarfarinManager.UI.Converters
{
    /// <summary>
    /// Converter da bool a Visibility con opzione di inversione
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Se true, inverte la logica (true -> Collapsed, false -> Visible)
        /// </summary>
        public bool Inverted { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolValue)
                return Visibility.Collapsed;

            bool result = Inverted ? !boolValue : boolValue;
            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
