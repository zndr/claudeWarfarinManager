using System;
using System.Globalization;
using System.Windows.Data;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.UI.Converters
{
    /// <summary>
    /// Converter per mostrare la descrizione italiana della sede anatomica dell'emorragia
    /// </summary>
    public class BleedingSiteToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SedeEmorragia sedeEmorragia)
            {
                return sedeEmorragia switch
                {
                    SedeEmorragia.Nessuna => "Non specificata",
                    SedeEmorragia.Cutanea => "Cutanea (ecchimosi, petecchie)",
                    SedeEmorragia.Nasale => "Nasale (epistassi)",
                    SedeEmorragia.Gengivale => "Gengivale",
                    SedeEmorragia.Gastrointestinale => "Gastrointestinale (melena, ematochezia)",
                    SedeEmorragia.Urinaria => "Urinaria (ematuria)",
                    SedeEmorragia.Intracranica => "🔴 Intracranica (cerebrale, subdurale, epidurale)",
                    SedeEmorragia.Retroperitoneale => "🔴 Retroperitoneale",
                    SedeEmorragia.Altra => "Altra sede",
                    _ => sedeEmorragia.ToString()
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
