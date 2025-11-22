namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Gravità dell'evento avverso
/// </summary>
public enum EventSeverity
{
    /// <summary>
    /// Evento minore (es. epistassi singola)
    /// </summary>
    Minor,
    
    /// <summary>
    /// Evento moderato (richiede attenzione medica)
    /// </summary>
    Moderate,
    
    /// <summary>
    /// Evento maggiore (Hb↓≥2 g/dL, trasfusione ≥2U, critico)
    /// </summary>
    Major,
    
    /// <summary>
    /// Evento fatale
    /// </summary>
    Fatal
}
