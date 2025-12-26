using System;
using System.Globalization;
using System.Windows.Data;

namespace WarfarinManager.UI.Converters
{
    /// <summary>
    /// Converte stringhe in int? (nullable)
    /// Gestisce correttamente l'input di numeri interi
    /// </summary>
    public class NullableIntConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue.ToString(CultureInfo.CurrentCulture);
            }

            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                // Se è vuoto, ritorna null
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return null;
                }

                stringValue = stringValue.Trim();

                // Prova a parsare come int
                if (int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.CurrentCulture, out int result))
                {
                    return result;
                }

                // Prova anche con InvariantCulture
                if (int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
                {
                    return result;
                }

                // Se fallisce, ritorna null
                return null;
            }

            return null;
        }
    }
}
