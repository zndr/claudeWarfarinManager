using System;
using System.Globalization;
using System.Windows.Data;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.UI.Converters
{
    /// <summary>
    /// Converter per mostrare la descrizione italiana del grado di certezza
    /// </summary>
    public class CertaintyLevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CertaintyLevel certaintyLevel)
            {
                return certaintyLevel switch
                {
                    CertaintyLevel.Certain => "Sicura",
                    CertaintyLevel.Doubtful => "Dubbia",
                    _ => certaintyLevel.ToString()
                };
            }

            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
