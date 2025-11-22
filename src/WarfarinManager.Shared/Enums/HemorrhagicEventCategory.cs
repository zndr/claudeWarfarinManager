namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Categoria specifica di evento emorragico
/// </summary>
public enum HemorrhagicEventCategory
{
    /// <summary>
    /// Emorragia maggiore (ISTH: Hb ↓≥2 g/dL, trasfusione ≥2U, critica, fatale)
    /// </summary>
    Major,
    
    /// <summary>
    /// Emorragia minore clinicamente rilevante
    /// </summary>
    MinorClinicallyRelevant,
    
    /// <summary>
    /// Emorragia minore
    /// </summary>
    Minor,
    
    /// <summary>
    /// Sanguinamento gastrointestinale
    /// </summary>
    Gastrointestinal,
    
    /// <summary>
    /// Ematuria
    /// </summary>
    Hematuria,
    
    /// <summary>
    /// Epistassi ricorrente
    /// </summary>
    Epistaxis,
    
    /// <summary>
    /// Ematoma spontaneo
    /// </summary>
    Hematoma,
    
    /// <summary>
    /// Emorragia intracranica
    /// </summary>
    Intracranial
}
