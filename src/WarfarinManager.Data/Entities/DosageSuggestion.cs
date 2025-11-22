using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Data.Entities;

/// <summary>
/// Entità suggerimento dosaggio (storico raccomandazioni)
/// </summary>
public class DosageSuggestion : BaseEntity
{
    /// <summary>
    /// ID controllo INR (Foreign Key)
    /// </summary>
    public int INRControlId { get; set; }
    
    /// <summary>
    /// Linea guida utilizzata per il calcolo
    /// </summary>
    public Guideline GuidelineUsed { get; set; }
    
    /// <summary>
    /// Nuova dose settimanale suggerita (mg)
    /// </summary>
    public decimal SuggestedWeeklyDose { get; set; }
    
    /// <summary>
    /// Descrizione azione dose carico (es. "+50% oggi")
    /// </summary>
    public string? LoadingDoseAction { get; set; }
    
    /// <summary>
    /// Percentuale aggiustamento rispetto a dose precedente
    /// </summary>
    public decimal? PercentageAdjustment { get; set; }
    
    /// <summary>
    /// Giorni al prossimo controllo raccomandato
    /// </summary>
    public int NextControlDays { get; set; }
    
    /// <summary>
    /// Richiede EBPM bridge
    /// </summary>
    public bool RequiresEBPM { get; set; }
    
    /// <summary>
    /// Richiede somministrazione Vitamina K
    /// </summary>
    public bool RequiresVitaminK { get; set; }
    
    /// <summary>
    /// Schema settimanale dettagliato (JSON con array 7 giorni)
    /// </summary>
    public string WeeklySchedule { get; set; } = string.Empty;
    
    /// <summary>
    /// Note cliniche e raccomandazioni
    /// </summary>
    public string? ClinicalNotes { get; set; }
    
    /// <summary>
    /// Testo formattato completo per export
    /// </summary>
    public string? ExportedText { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// Controllo INR associato
    /// </summary>
    public INRControl INRControl { get; set; } = null!;
}
