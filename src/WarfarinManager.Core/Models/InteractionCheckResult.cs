using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Core.Models;

/// <summary>
/// Risultato verifica interazione farmacologica
/// </summary>
public class InteractionCheckResult
{
    /// <summary>
    /// Presenza interazione documentata
    /// </summary>
    public bool HasInteraction { get; set; }
    
    /// <summary>
    /// Livello rischio interazione
    /// </summary>
    public InteractionLevel InteractionLevel { get; set; }
    
    /// <summary>
    /// Nome farmaco
    /// </summary>
    public string MedicationName { get; set; } = string.Empty;
    
    /// <summary>
    /// Effetto interazione (Increases/Decreases/Variable)
    /// </summary>
    public string InteractionEffect { get; set; } = string.Empty;
    
    /// <summary>
    /// Meccanismo interazione
    /// </summary>
    public string Mechanism { get; set; } = string.Empty;
    
    /// <summary>
    /// Odds Ratio sanguinamento
    /// </summary>
    public decimal? OddsRatio { get; set; }
    
    /// <summary>
    /// Gestione FCSA-SIMG
    /// </summary>
    public string FCSAManagement { get; set; } = string.Empty;
    
    /// <summary>
    /// Gestione ACCP/ACC
    /// </summary>
    public string ACCPManagement { get; set; } = string.Empty;
    
    /// <summary>
    /// Giorni raccomandati per controllo INR
    /// </summary>
    public int? RecommendedINRCheckDays { get; set; }
    
    /// <summary>
    /// Messaggio descrittivo
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Raccomandazione aggiustamento dose
/// </summary>
public class DoseAdjustmentRecommendation
{
    /// <summary>
    /// Linea guida utilizzata
    /// </summary>
    public GuidelineType GuidelineUsed { get; set; }
    
    /// <summary>
    /// Percentuale aggiustamento suggerita
    /// </summary>
    public decimal? PercentageAdjustment { get; set; }
    
    /// <summary>
    /// Nuova dose settimanale suggerita (mg)
    /// </summary>
    public decimal? SuggestedWeeklyDoseMg { get; set; }
    
    /// <summary>
    /// Raccomandazione testuale
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;
    
    /// <summary>
    /// Giorni raccomandati per controllo INR
    /// </summary>
    public int? RecommendedINRCheckDays { get; set; }
    
    /// <summary>
    /// Note aggiuntive
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
