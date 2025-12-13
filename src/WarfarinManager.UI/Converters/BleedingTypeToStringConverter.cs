using System;
using System.Globalization;
using System.Windows.Data;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.UI.Converters
{
    /// <summary>
    /// Converter per mostrare la descrizione italiana del tipo di emorragia
    /// </summary>
    public class BleedingTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TipoEmorragia tipoEmorragia)
            {
                return tipoEmorragia switch
                {
                    TipoEmorragia.Nessuna => "Nessun sanguinamento",
                    TipoEmorragia.Minore => "Emorragia Minore (controllabile localmente)",
                    TipoEmorragia.Maggiore => "Emorragia Maggiore (richiede ricovero)",
                    TipoEmorragia.RischioVitale => "🔴 Emorragia con Rischio Vitale (EMERGENZA)",
                    _ => tipoEmorragia.ToString()
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
