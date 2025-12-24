namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Tipo di terapia anticoagulante per l'importazione pazienti
/// </summary>
public enum TherapyType
{
    /// <summary>
    /// Solo Warfarin (B01AA03)
    /// </summary>
    Warfarin,

    /// <summary>
    /// Solo DOAC (B01AE07, B01AF01, B01AF02, B01AF03)
    /// </summary>
    DOAC,

    /// <summary>
    /// Entrambi Warfarin e DOAC
    /// </summary>
    Both
}
