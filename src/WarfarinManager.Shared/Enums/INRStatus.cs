namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Stato INR rispetto al range terapeutico
/// </summary>
public enum INRStatus
{
    /// <summary>
    /// INR nel range terapeutico
    /// </summary>
    InRange,
    
    /// <summary>
    /// INR sotto il range terapeutico (sotto-anticoagulazione)
    /// </summary>
    BelowRange,
    
    /// <summary>
    /// INR sopra il range terapeutico (sovra-anticoagulazione)
    /// </summary>
    AboveRange
}
