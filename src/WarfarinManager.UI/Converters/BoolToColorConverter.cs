using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WarfarinManager.UI.Converters
{
    /// <summary>
    /// Converter da bool a Color per bordiarancioni in modalità edit
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEdit && isEdit)
            {
                // Arancione per modalità edit
                return Color.FromRgb(255, 152, 0); // #FF9800
            }

            // Blu default (stesso colore del tema)
            return Color.FromRgb(33, 150, 243); // #2196F3
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
