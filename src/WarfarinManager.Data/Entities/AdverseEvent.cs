using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Data.Entities;

/// <summary>
/// Entità evento avverso
/// </summary>
public class AdverseEvent : BaseEntity
{
    /// <summary>
    /// ID paziente (Foreign Key)
    /// </summary>
    public int PatientId { get; set; }
    
    /// <summary>
    /// Data evento
    /// </summary>
    public DateTime EventDate { get; set; }
    
    /// <summary>
    /// Tipo evento (Emorragico/Trombotico)
    /// </summary>
    public AdverseEventType EventType { get; set; }
    
    /// <summary>
    /// Categoria emorragica (se EventType = Hemorrhagic)
    /// </summary>
    public HemorrhagicEventCategory? HemorrhagicCategory { get; set; }
    
    /// <summary>
    /// Categoria trombotica (se EventType = Thrombotic)
    /// </summary>
    public ThromboticEventCategory? ThromboticCategory { get; set; }
    
    /// <summary>
    /// Gravità evento
    /// </summary>
    public EventSeverity Severity { get; set; }
    
    /// <summary>
    /// INR al momento dell'evento (se disponibile)
    /// </summary>
    public decimal? INRAtEvent { get; set; }
    
    /// <summary>
    /// Dose settimanale in uso al momento dell'evento (mg)
    /// </summary>
    public decimal? WeeklyDoseAtEvent { get; set; }
    
    /// <summary>
    /// Gestione evento (testo libero)
    /// </summary>
    public string? Management { get; set; }
    
    /// <summary>
    /// Outcome evento
    /// </summary>
    public string? Outcome { get; set; }
    
    /// <summary>
    /// Note cliniche
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// ID controllo INR collegato (opzionale)
    /// </summary>
    public int? LinkedINRControlId { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// Paziente associato
    /// </summary>
    public Patient Patient { get; set; } = null!;
    
    /// <summary>
    /// Controllo INR collegato (se presente)
    /// </summary>
    public INRControl? LinkedINRControl { get; set; }
}
