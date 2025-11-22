namespace WarfarinManager.Core.Models;

/// <summary>
/// Risultato calcolo TTR
/// </summary>
public class TTRResult
{
    /// <summary>
    /// Percentuale TTR (0-100)
    /// </summary>
    public decimal TTRPercentage { get; set; }
    
    /// <summary>
    /// Giorni totali analizzati
    /// </summary>
    public int TotalDays { get; set; }
    
    /// <summary>
    /// Giorni in range terapeutico
    /// </summary>
    public int DaysInRange { get; set; }
    
    /// <summary>
    /// Giorni sotto range
    /// </summary>
    public int DaysBelowRange { get; set; }
    
    /// <summary>
    /// Giorni sopra range
    /// </summary>
    public int DaysAboveRange { get; set; }
    
    /// <summary>
    /// Valutazione qualitativa TTR
    /// </summary>
    public TTRQuality Quality { get; set; }
    
    /// <summary>
    /// Data inizio periodo analizzato
    /// </summary>
    public DateTime PeriodStart { get; set; }
    
    /// <summary>
    /// Data fine periodo analizzato
    /// </summary>
    public DateTime PeriodEnd { get; set; }
    
    /// <summary>
    /// Numero controlli INR nel periodo
    /// </summary>
    public int NumberOfControls { get; set; }
}

/// <summary>
/// Qualità del TTR
/// </summary>
public enum TTRQuality
{
    /// <summary>
    /// TTR ≥70% - Eccellente
    /// </summary>
    Excellent,
    
    /// <summary>
    /// TTR 60-69% - Accettabile
    /// </summary>
    Acceptable,
    
    /// <summary>
    /// TTR 50-59% - Subottimale
    /// </summary>
    Suboptimal,
    
    /// <summary>
    /// TTR <50% - Critico
    /// </summary>
    Critical
}
