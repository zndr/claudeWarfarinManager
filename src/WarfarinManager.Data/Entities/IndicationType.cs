using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Data.Entities;

/// <summary>
/// Entità tipo di indicazione terapeutica (tabella lookup)
/// </summary>
public class IndicationType
{
    /// <summary>
    /// Identificativo univoco
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Codice univoco indicazione (es. "FA_STROKE", "TVP_TREATMENT")
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Categoria (es. "TROMBOEMBOLISMO VENOSO", "FIBRILLAZIONE ATRIALE")
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrizione completa
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Target INR minimo
    /// </summary>
    public decimal TargetINRMin { get; set; }
    
    /// <summary>
    /// Target INR massimo
    /// </summary>
    public decimal TargetINRMax { get; set; }
    
    /// <summary>
    /// Durata tipica terapia (descrittiva, es. "3-6 mesi", "indefinito")
    /// </summary>
    public string? TypicalDuration { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// Indicazioni che usano questo tipo
    /// </summary>
    public ICollection<Indication> Indications { get; set; } = new List<Indication>();
}
