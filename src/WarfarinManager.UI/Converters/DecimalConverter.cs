using System;
using System.Globalization;
using System.Windows.Data;

namespace WarfarinManager.UI.Converters
{
    /// <summary>
    /// Converte stringhe con virgola o punto in decimal
    /// </summary>
    public class DecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                // Se è zero, mostra stringa vuota invece di "0,0"
                if (decimalValue == 0)
                    return string.Empty;
                
                // Altrimenti mostra il numero senza decimali se è intero
                if (decimalValue == Math.Floor(decimalValue))
                    return decimalValue.ToString("0", CultureInfo.CurrentCulture);
                
                return decimalValue.ToString("0.0", CultureInfo.CurrentCulture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Debug.WriteLine($"[DecimalConverter.ConvertBack] Input value={value}, type={value?.GetType().Name}");

            if (value is string stringValue)
            {
                // Se è vuoto, ritorna 0
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    System.Diagnostics.Debug.WriteLine($"[DecimalConverter.ConvertBack] Empty string, returning 0");
                    return 0m;
                }

                // Sostituisci virgola con punto per parsing
                stringValue = stringValue.Replace(',', '.');

                if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                {
                    System.Diagnostics.Debug.WriteLine($"[DecimalConverter.ConvertBack] Parsed successfully: {result}");
                    return result;
                }

                System.Diagnostics.Debug.WriteLine($"[DecimalConverter.ConvertBack] Failed to parse, returning 0");
            }
            return 0m;
        }
    }
}
