using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.UI.Converters
{
    /// <summary>
    /// Converter per mostrare la descrizione italiana del tipo di reazione avversa
    /// </summary>
    public class AdverseReactionTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AdverseReactionType reactionType)
            {
                var field = reactionType.GetType().GetField(reactionType.ToString());
                var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
                return attribute?.Description ?? reactionType.ToString();
            }

            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
