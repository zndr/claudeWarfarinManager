using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Core.Models;

/// <summary>
/// Risultato del calcolo suggerimento dosaggio
/// </summary>
public class DosageSuggestionResult
{
    /// <summary>
    /// Linea guida utilizzata
    /// </summary>
    public Guideline GuidelineUsed { get; set; }
    
    /// <summary>
    /// INR è nel range terapeutico
    /// </summary>
    public bool IsInRange { get; set; }
    
    /// <summary>
    /// Stato INR (es. "In range", "Sottoterapeutico", "Sovraterapeutico")
    /// </summary>
    public string INRStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Distanza da target (positivo se sopra, negativo se sotto)
    /// </summary>
    public decimal DistanceFromTarget { get; set; }
    
    /// <summary>
    /// Azione dose carico (se necessaria)
    /// </summary>
    public string? LoadingDoseAction { get; set; }
    
    /// <summary>
    /// Percentuale aggiustamento dose settimanale
    /// </summary>
    public decimal? PercentageAdjustment { get; set; }
    
    /// <summary>
    /// Nuova dose settimanale suggerita (mg)
    /// </summary>
    public decimal SuggestedWeeklyDose { get; set; }
    
    /// <summary>
    /// Schema settimanale dettagliato
    /// </summary>
    public List<DailyDoseSchedule> WeeklySchedule { get; set; } = new();
    
    /// <summary>
    /// Giorni al prossimo controllo
    /// </summary>
    public int NextControlDays { get; set; }
    
    /// <summary>
    /// Data prossimo controllo suggerito
    /// </summary>
    public DateTime NextControlDate { get; set; }
    
    /// <summary>
    /// Motivazione timing controllo
    /// </summary>
    public string ControlRationale { get; set; } = string.Empty;
    
    /// <summary>
    /// Richiede EBPM bridge
    /// </summary>
    public bool RequiresEBPM { get; set; }
    
    /// <summary>
    /// Dettagli EBPM se necessario
    /// </summary>
    public string? EBPMDetails { get; set; }
    
    /// <summary>
    /// Richiede Vitamina K
    /// </summary>
    public bool RequiresVitaminK { get; set; }
    
    /// <summary>
    /// Dosaggio Vitamina K se necessaria
    /// </summary>
    public string? VitaminKDosage { get; set; }
    
    /// <summary>
    /// Note cliniche e raccomandazioni
    /// </summary>
    public List<string> ClinicalNotes { get; set; } = new();
    
    /// <summary>
    /// Alert e warning
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Dose giornaliera nello schema settimanale
/// </summary>
public class DailyDoseSchedule
{
    public int DayOfWeek { get; set; } // 1=Lunedì, 7=Domenica
    public string DayName { get; set; } = string.Empty;
    public decimal DoseMg { get; set; }
    public string DoseDescription { get; set; } = string.Empty; // es. "1 cp 5mg", "1/2 cp 2.5mg"
}
