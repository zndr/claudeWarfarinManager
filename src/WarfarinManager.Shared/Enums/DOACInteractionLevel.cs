namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Livello di pericolosità dell'interazione farmacologica con DOAC
/// </summary>
public enum DOACInteractionLevel
{
    /// <summary>
    /// Nessuna interazione clinicamente rilevante
    /// </summary>
    None,

    /// <summary>
    /// Interazione lieve - monitoraggio raccomandato
    /// </summary>
    Minor,

    /// <summary>
    /// Interazione moderata - cautela e aggiustamento dose
    /// </summary>
    Moderate,

    /// <summary>
    /// Interazione pericolosa - controindicazione o riduzione dose obbligatoria
    /// </summary>
    Dangerous,

    /// <summary>
    /// Controindicazione assoluta
    /// </summary>
    Contraindicated
}
