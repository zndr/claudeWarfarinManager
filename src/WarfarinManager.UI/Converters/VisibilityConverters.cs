using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WarfarinManager.UI.Converters;

/// <summary>
/// Converte un conteggio in Visibility:
/// - 0 = Visible (mostra messaggio "lista vuota")
/// - >0 = Collapsed
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converte un conteggio in Visibility (inverso):
/// - 0 = Collapsed
/// - >0 = Visible
/// </summary>
public class CountToVisibilityInverseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converte una stringa in Visibility:
/// - null o vuota = Collapsed
/// - non vuota = Visible
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converte un bool? in bool per RadioButton, con supporto per null
/// ConverterParameter: "true", "false", o null per il valore da matchare
/// </summary>
public class NullableBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = value as bool?;
        
        if (parameter == null)
        {
            return boolValue == null;
        }
        
        if (bool.TryParse(parameter.ToString(), out bool paramBool))
        {
            return boolValue == paramBool;
        }
        
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked)
        {
            if (parameter == null)
            {
                return null;
            }
            
            if (bool.TryParse(parameter.ToString(), out bool paramBool))
            {
                return paramBool;
            }
        }
        
        return Binding.DoNothing;
    }
}

/// <summary>
/// Converte bool in Visibility inverso:
/// - true = Collapsed
/// - false = Visible
/// </summary>
public class InverseBoolToVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converte una percentuale (0-100) in larghezza basata su un valore massimo.
/// ConverterParameter: larghezza massima in pixel (default 100)
/// </summary>
public class PercentageToWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        decimal percentage = 0;
        double maxWidth = 100;

        // Ottieni la percentuale
        if (value is decimal decValue)
            percentage = decValue;
        else if (value is double dblValue)
            percentage = (decimal)dblValue;
        else if (value is int intValue)
            percentage = intValue;

        // Ottieni larghezza massima dal parametro
        if (parameter != null && double.TryParse(parameter.ToString(), out double parsedWidth))
        {
            maxWidth = parsedWidth;
        }

        // Calcola larghezza proporzionale (clamp a 0-100)
        percentage = Math.Max(0, Math.Min(100, percentage));
        return (double)percentage / 100.0 * maxWidth;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
