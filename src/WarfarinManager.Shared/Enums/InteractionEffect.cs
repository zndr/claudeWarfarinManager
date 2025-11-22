namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Effetto del farmaco sui livelli di INR
/// </summary>
public enum InteractionEffect
{
    /// <summary>
    /// Aumenta INR (aumenta effetto anticoagulante)
    /// </summary>
    Increases,
    
    /// <summary>
    /// Diminuisce INR (riduce effetto anticoagulante)
    /// </summary>
    Decreases,
    
    /// <summary>
    /// Effetto variabile o non prevedibile
    /// </summary>
    Variable
}
