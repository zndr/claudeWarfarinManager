namespace WarfarinManager.Data.Entities;

/// <summary>
/// Classe base per tutte le entità
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identificativo univoco
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Data di creazione record
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Data ultimo aggiornamento
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
