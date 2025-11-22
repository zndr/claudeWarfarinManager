namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Fase della terapia anticoagulante
/// </summary>
public enum TherapyPhase
{
    /// <summary>
    /// Fase di induzione (prime settimane)
    /// </summary>
    Induction,
    
    /// <summary>
    /// Fase di mantenimento (paziente stabilizzato)
    /// </summary>
    Maintenance,
    
    /// <summary>
    /// Post-aggiustamento dose (monitoraggio dopo modifica)
    /// </summary>
    PostAdjustment
}
