using System;
using System.Globalization;
using System.Windows.Data;

namespace WarfarinManager.UI.Converters;

/// <summary>
/// Converter per bindare RadioButton a proprietà enum.
/// Restituisce true se il valore corrisponde al parametro.
/// </summary>
public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        string enumValue = value.ToString()!;
        string targetValue = parameter.ToString()!;
        
        return enumValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter != null)
        {
            return Enum.Parse(targetType, parameter.ToString()!);
        }
        
        return Binding.DoNothing;
    }
}
