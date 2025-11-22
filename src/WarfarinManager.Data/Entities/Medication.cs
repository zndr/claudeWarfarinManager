using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Data.Entities;

/// <summary>
/// Entità farmaco concomitante
/// </summary>
public class Medication : BaseEntity
{
    /// <summary>
    /// ID paziente (Foreign Key)
    /// </summary>
    public int PatientId { get; set; }
    
    /// <summary>
    /// Nome farmaco
    /// </summary>
    public string MedicationName { get; set; } = string.Empty;
    
    /// <summary>
    /// Dosaggio
    /// </summary>
    public string? Dosage { get; set; }
    
    /// <summary>
    /// Frequenza assunzione
    /// </summary>
    public string? Frequency { get; set; }
    
    /// <summary>
    /// Data inizio terapia
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Data fine terapia (null se attiva)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Terapia attiva
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Livello interazione con warfarin (calcolato)
    /// </summary>
    public InteractionLevel InteractionLevel { get; set; }
    
    /// <summary>
    /// Dettagli interazione (JSON con raccomandazioni)
    /// </summary>
    public string? InteractionDetails { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// Paziente associato
    /// </summary>
    public Patient Patient { get; set; } = null!;
}
