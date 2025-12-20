using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.UI.Converters
{
    /// <summary>
    /// Converte AdverseReactionSeverity in un colore di sfondo appropriato
    /// </summary>
    public class SeverityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AdverseReactionSeverity severity)
            {
                return severity switch
                {
                    AdverseReactionSeverity.Critical => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D13438")), // Rosso
                    AdverseReactionSeverity.Serious => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB900")),  // Arancione
                    AdverseReactionSeverity.Common => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#107C10")),   // Verde
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }

            // Se il valore è una stringa, prova a parsarla
            if (value is string severityString && Enum.TryParse<AdverseReactionSeverity>(severityString, out var parsedSeverity))
            {
                return Convert(parsedSeverity, targetType, parameter, culture);
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
