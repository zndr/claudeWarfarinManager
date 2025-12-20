namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Esito della valutazione eleggibilità TAO
/// </summary>
public enum TAOEligibility
{
    /// <summary>
    /// TAO indicata - nessuna controindicazione
    /// </summary>
    Indicated,
    
    /// <summary>
    /// TAO indicata con cautela - controindicazioni relative presenti
    /// </summary>
    IndicatedWithCaution,
    
    /// <summary>
    /// TAO non indicata - score troppo basso
    /// </summary>
    NotIndicatedLowRisk,
    
    /// <summary>
    /// TAO controindicata - controindicazioni assolute presenti
    /// </summary>
    Contraindicated,
    
    /// <summary>
    /// Valutazione in corso / incompleta
    /// </summary>
    Pending
}
