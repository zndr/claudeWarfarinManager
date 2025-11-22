using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Data.Entities;

/// <summary>
/// Entità farmaco con interazione con warfarin (tabella lookup)
/// </summary>
public class InteractionDrug
{
    /// <summary>
    /// Identificativo univoco
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome farmaco
    /// </summary>
    public string DrugName { get; set; } = string.Empty;
    
    /// <summary>
    /// Categoria farmacologica (es. "Antibiotic", "Antiarrhythmic")
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Effetto sull'INR
    /// </summary>
    public InteractionEffect InteractionEffect { get; set; }
    
    /// <summary>
    /// Odds Ratio per sanguinamento (se disponibile)
    /// </summary>
    public decimal? OddsRatio { get; set; }
    
    /// <summary>
    /// Meccanismo interazione
    /// </summary>
    public string? Mechanism { get; set; }
    
    /// <summary>
    /// Raccomandazione gestione secondo FCSA
    /// </summary>
    public string? FCSAManagement { get; set; }
    
    /// <summary>
    /// Raccomandazione gestione secondo ACCP
    /// </summary>
    public string? ACCPManagement { get; set; }
    
    /// <summary>
    /// Giorni raccomandati per controllo INR dopo inizio/modifica
    /// </summary>
    public int? RecommendedINRCheckDays { get; set; }
    
    /// <summary>
    /// Livello interazione
    /// </summary>
    public InteractionLevel InteractionLevel { get; set; }
}
