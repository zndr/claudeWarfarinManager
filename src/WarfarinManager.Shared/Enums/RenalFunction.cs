namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Classificazione funzione renale (eGFR mL/min)
/// </summary>
public enum RenalFunction
{
    /// <summary>
    /// eGFR ≥80 mL/min - Normale
    /// </summary>
    Normal,

    /// <summary>
    /// eGFR 50-79 mL/min - Lievemente ridotta
    /// </summary>
    MildlyReduced,

    /// <summary>
    /// eGFR 30-49 mL/min - Moderatamente ridotta
    /// </summary>
    ModeratelyReduced,

    /// <summary>
    /// eGFR 15-29 mL/min - Severamente ridotta
    /// </summary>
    SeverelyReduced,

    /// <summary>
    /// eGFR &lt;15 mL/min o dialisi - Insufficienza renale terminale
    /// </summary>
    EndStage
}
