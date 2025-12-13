namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Classificazione della gravità dell'emorragia in paziente con INR elevato
/// Basato su criteri ISTH (International Society on Thrombosis and Haemostasis)
/// </summary>
public enum TipoEmorragia
{
    /// <summary>
    /// Nessun sanguinamento presente
    /// </summary>
    Nessuna = 0,

    /// <summary>
    /// Emorragia minore
    /// - Sanguinamento controllabile con misure locali
    /// - Sede: cutanea, nasale, gengivale, urinaria lieve
    /// - Non richiede trasfusione
    /// - Calo Hb &lt;2 g/dL
    /// </summary>
    Minore = 1,

    /// <summary>
    /// Emorragia maggiore
    /// - Richiede trasfusione (≥2 unità emazie)
    /// - Calo Hb ≥2 g/dL
    /// - Sede: GI con sanguinamento attivo, urinaria importante
    /// - Ematoma significativo con compromissione funzionale
    /// - RICHIEDE RICOVERO
    /// </summary>
    Maggiore = 2,

    /// <summary>
    /// Emorragia con rischio vitale
    /// - Sede critica: intracranica, retroperitoneale
    /// - Shock emorragico
    /// - Rischio mortalità elevato
    /// - EMERGENZA MEDICA - Richiede terapia intensiva
    /// </summary>
    RischioVitale = 3
}
