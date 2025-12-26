using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.UI.Converters
{
    /// <summary>
    /// Converte null/non-null in Visibility (non-null = Visible, null = Collapsed)
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = value != null && !string.IsNullOrEmpty(value.ToString());
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte una lista di stringhe in una singola stringa separata da virgole
    /// </summary>
    public class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<string> list)
            {
                return string.Join(", ", list);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte il livello di interazione DOAC in testo descrittivo
    /// </summary>
    public class InteractionLevelToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DOACInteractionLevel level)
            {
                return level switch
                {
                    DOACInteractionLevel.Contraindicated => "CONTROINDICATO",
                    DOACInteractionLevel.Dangerous => "EVITARE / NON RACCOMANDATO",
                    DOACInteractionLevel.Moderate => "CAUTELA",
                    DOACInteractionLevel.Minor => "MONITORARE",
                    DOACInteractionLevel.None => "OK",
                    _ => level.ToString()
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte il livello di interazione DOAC in colore di sfondo
    /// </summary>
    public class InteractionLevelToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DOACInteractionLevel level)
            {
                return level switch
                {
                    DOACInteractionLevel.Contraindicated => new SolidColorBrush(Color.FromRgb(255, 107, 107)),
                    DOACInteractionLevel.Dangerous => new SolidColorBrush(Color.FromRgb(255, 204, 102)),
                    DOACInteractionLevel.Moderate => new SolidColorBrush(Color.FromRgb(111, 181, 255)),
                    DOACInteractionLevel.Minor => new SolidColorBrush(Color.FromRgb(126, 231, 135)),
                    DOACInteractionLevel.None => new SolidColorBrush(Color.FromRgb(126, 231, 135)),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte il livello di interazione DOAC in colore del bordo
    /// </summary>
    public class InteractionLevelToBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DOACInteractionLevel level)
            {
                return level switch
                {
                    DOACInteractionLevel.Contraindicated => new SolidColorBrush(Color.FromRgb(200, 50, 50)),
                    DOACInteractionLevel.Dangerous => new SolidColorBrush(Color.FromRgb(200, 150, 50)),
                    DOACInteractionLevel.Moderate => new SolidColorBrush(Color.FromRgb(50, 130, 200)),
                    DOACInteractionLevel.Minor => new SolidColorBrush(Color.FromRgb(50, 180, 80)),
                    DOACInteractionLevel.None => new SolidColorBrush(Color.FromRgb(50, 180, 80)),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte il livello di interazione DOAC in colore del testo
    /// </summary>
    public class InteractionLevelToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DOACInteractionLevel level)
            {
                return level switch
                {
                    DOACInteractionLevel.Contraindicated => new SolidColorBrush(Color.FromRgb(80, 0, 0)),
                    DOACInteractionLevel.Dangerous => new SolidColorBrush(Color.FromRgb(80, 50, 0)),
                    DOACInteractionLevel.Moderate => new SolidColorBrush(Color.FromRgb(0, 40, 80)),
                    DOACInteractionLevel.Minor => new SolidColorBrush(Color.FromRgb(0, 60, 20)),
                    DOACInteractionLevel.None => new SolidColorBrush(Color.FromRgb(0, 60, 20)),
                    _ => new SolidColorBrush(Colors.White)
                };
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte booleano IsMapped in colore bordo chip
    /// </summary>
    public class IsMappedToChipBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMapped)
            {
                return isMapped
                    ? new SolidColorBrush(Color.FromArgb(153, 126, 231, 135))  // Verde per mappato
                    : new SolidColorBrush(Color.FromArgb(166, 255, 107, 107)); // Rosso per non mappato
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
