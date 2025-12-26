using System;
using System.Globalization;
using System.Windows.Data;

namespace WarfarinManager.UI.Converters
{
    /// <summary>
    /// Converte stringhe con virgola o punto in decimal? (nullable)
    /// Gestisce correttamente l'input di numeri decimali in italiano
    /// </summary>
    public class NullableDecimalConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                // Determina il numero di decimali dal parametro (default: 2)
                int decimals = 2;
                if (parameter is string paramStr && int.TryParse(paramStr, out int parsedDecimals))
                {
                    decimals = parsedDecimals;
                }

                // Se è intero, mostra senza decimali
                if (decimalValue == Math.Floor(decimalValue))
                    return decimalValue.ToString("0", CultureInfo.CurrentCulture);

                // Altrimenti mostra con i decimali specificati
                string format = decimals == 1 ? "0.#" : "0.##";
                return decimalValue.ToString(format, CultureInfo.CurrentCulture);
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

                // Normalizza: sostituisci virgola con punto per parsing
                stringValue = stringValue.Trim().Replace(',', '.');

                // Prova a parsare come decimal
                if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
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
