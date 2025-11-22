namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Livello di interazione farmacologica
/// </summary>
public enum InteractionLevel
{
    /// <summary>
    /// Nessuna interazione nota
    /// </summary>
    None,
    
    /// <summary>
    /// Interazione bassa (monitoraggio routine)
    /// </summary>
    Low,
    
    /// <summary>
    /// Interazione moderata (monitoraggio ravvicinato)
    /// </summary>
    Moderate,
    
    /// <summary>
    /// Interazione alta (riduzione dose empirica, controllo ravvicinato)
    /// </summary>
    High
}
