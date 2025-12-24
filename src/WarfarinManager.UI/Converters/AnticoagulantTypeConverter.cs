using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WarfarinManager.Shared.Constants;

namespace WarfarinManager.UI.Converters;

/// <summary>
/// Converte AnticoagulantType in abbreviazione (W/A/D/E/R/-)
/// </summary>
public class AnticoagulantTypeToAbbreviationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return AnticoagulantTypes.GetAbbreviation(value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ConvertBack non supportato per AnticoagulantTypeToAbbreviationConverter");
    }
}

/// <summary>
/// Converte AnticoagulantType in nome completo (es. "Apixaban (Eliquis)")
/// </summary>
public class AnticoagulantTypeToDisplayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return AnticoagulantTypes.GetDisplayName(value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ConvertBack non supportato per AnticoagulantTypeToDisplayNameConverter");
    }
}

/// <summary>
/// Converte AnticoagulantType in Visibility:
/// - Warfarin → Visible
/// - Altro → Collapsed
/// - null → Visible (backward compatibility: pazienti legacy assumono Warfarin)
/// </summary>
public class AnticoagulantTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string anticoagulantType)
        {
            return AnticoagulantTypes.IsWarfarin(anticoagulantType)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // null = paziente vecchio senza AnticoagulantType → Assumiamo Warfarin (backward compatibility)
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ConvertBack non supportato per AnticoagulantTypeToVisibilityConverter");
    }
}

/// <summary>
/// Converte AnticoagulantType in Visibility inverso:
/// - DOAC → Visible
/// - Warfarin → Collapsed
/// - null → Collapsed (backward compatibility: pazienti legacy non sono DOACs)
/// </summary>
public class DoacTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string anticoagulantType)
        {
            return AnticoagulantTypes.IsDoac(anticoagulantType)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // null = paziente vecchio → Non DOAC
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ConvertBack non supportato per DoacTypeToVisibilityConverter");
    }
}
