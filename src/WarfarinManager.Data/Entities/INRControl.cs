using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Data.Entities;

/// <summary>
/// Entità controllo INR
/// </summary>
public class INRControl : BaseEntity
{
    /// <summary>
    /// ID paziente (Foreign Key)
    /// </summary>
    public int PatientId { get; set; }
    
    /// <summary>
    /// Data prelievo/controllo
    /// </summary>
    public DateTime ControlDate { get; set; }
    
    /// <summary>
    /// Valore INR rilevato
    /// </summary>
    public decimal INRValue { get; set; }
    
    /// <summary>
    /// Dose settimanale corrente in mg
    /// </summary>
    public decimal CurrentWeeklyDose { get; set; }
    
    /// <summary>
    /// Fase della terapia
    /// </summary>
    public TherapyPhase PhaseOfTherapy { get; set; }
    
    /// <summary>
    /// Paziente compliant (assume regolarmente)
    /// </summary>
    public bool IsCompliant { get; set; }
    
    /// <summary>
    /// Note libere
    /// </summary>
    public string? Notes { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// Paziente associato
    /// </summary>
    public Patient Patient { get; set; } = null!;
    
    /// <summary>
    /// Dosi giornaliere della settimana
    /// </summary>
    public ICollection<DailyDose> DailyDoses { get; set; } = new List<DailyDose>();
    
    /// <summary>
    /// Suggerimenti dosaggio generati
    /// </summary>
    public ICollection<DosageSuggestion> DosageSuggestions { get; set; } = new List<DosageSuggestion>();
}
