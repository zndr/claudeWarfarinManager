namespace WarfarinManager.Data.Entities;

/// <summary>
/// Entità indicazione terapeutica
/// </summary>
public class Indication : BaseEntity
{
    /// <summary>
    /// ID paziente (Foreign Key)
    /// </summary>
    public int PatientId { get; set; }
    
    /// <summary>
    /// Codice tipo indicazione (riferimento a IndicationType)
    /// </summary>
    public string IndicationTypeCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Target INR minimo
    /// </summary>
    public decimal TargetINRMin { get; set; }
    
    /// <summary>
    /// Target INR massimo
    /// </summary>
    public decimal TargetINRMax { get; set; }
    
    /// <summary>
    /// Data inizio indicazione
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Data fine indicazione (null se attiva)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Indicazione attualmente attiva (solo una per paziente)
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Motivazione cambio indicazione
    /// </summary>
    public string? ChangeReason { get; set; }
    
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
    /// Tipo indicazione (lookup)
    /// </summary>
    public IndicationType IndicationType { get; set; } = null!;
}
