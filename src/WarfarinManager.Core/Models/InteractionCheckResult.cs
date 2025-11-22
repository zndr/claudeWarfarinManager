using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Core.Models;

/// <summary>
/// Risultato verifica interazione farmacologica
/// </summary>
public class InteractionCheckResult
{
    /// <summary>
    /// Interazione trovata
    /// </summary>
    public bool InteractionFound { get; set; }
    
    /// <summary>
    /// Nome farmaco
    /// </summary>
    public string DrugName { get; set; } = string.Empty;
    
    /// <summary>
    /// Livello interazione
    /// </summary>
    public InteractionLevel InteractionLevel { get; set; }
    
    /// <summary>
    /// Effetto sull'INR
    /// </summary>
    public InteractionEffect? InteractionEffect { get; set; }
    
    /// <summary>
    /// Odds Ratio per sanguinamento (se disponibile)
    /// </summary>
    public decimal? OddsRatio { get; set; }
    
    /// <summary>
    /// Meccanismo interazione
    /// </summary>
    public string? Mechanism { get; set; }
    
    /// <summary>
    /// Raccomandazione gestione FCSA
    /// </summary>
    public string? FCSAManagement { get; set; }
    
    /// <summary>
    /// Raccomandazione gestione ACCP
    /// </summary>
    public string? ACCPManagement { get; set; }
    
    /// <summary>
    /// Giorni raccomandati per controllo INR
    /// </summary>
    public int? RecommendedINRCheckDays { get; set; }
    
    /// <summary>
    /// Percentuale riduzione dose empirica suggerita (se aumenta INR)
    /// </summary>
    public decimal? SuggestedDoseReduction { get; set; }
    
    /// <summary>
    /// Percentuale aumento dose empirica suggerita (se riduce INR)
    /// </summary>
    public decimal? SuggestedDoseIncrease { get; set; }
}
