using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Data.Entities;

/// <summary>
/// Entità paziente
/// </summary>
public class Patient : BaseEntity
{
    /// <summary>
    /// Nome
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Cognome
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Data di nascita
    /// </summary>
    public DateTime BirthDate { get; set; }
    
    /// <summary>
    /// Codice fiscale italiano (16 caratteri)
    /// </summary>
    public string FiscalCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Sesso
    /// </summary>
    public Gender? Gender { get; set; }
    
    /// <summary>
    /// Telefono
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Indirizzo completo
    /// </summary>
    public string? Address { get; set; }
    
    /// <summary>
    /// Note anamnestiche libere
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Flag metabolizzatore lento (calcolato automaticamente se dose settimanale <15 mg)
    /// </summary>
    public bool IsSlowMetabolizer { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// Indicazioni terapeutiche del paziente
    /// </summary>
    public ICollection<Indication> Indications { get; set; } = new List<Indication>();
    
    /// <summary>
    /// Farmaci concomitanti
    /// </summary>
    public ICollection<Medication> Medications { get; set; } = new List<Medication>();
    
    /// <summary>
    /// Controlli INR
    /// </summary>
    public ICollection<INRControl> INRControls { get; set; } = new List<INRControl>();
    
    /// <summary>
    /// Eventi avversi
    /// </summary>
    public ICollection<AdverseEvent> AdverseEvents { get; set; } = new List<AdverseEvent>();
    
    /// <summary>
    /// Piani di bridge therapy
    /// </summary>
    public ICollection<BridgeTherapyPlan> BridgeTherapyPlans { get; set; } = new List<BridgeTherapyPlan>();
    
    // Computed Properties (non mappate su DB)
    
    /// <summary>
    /// Età calcolata in anni
    /// </summary>
    public int Age => DateTime.Today.Year - BirthDate.Year - 
                     (DateTime.Today.DayOfYear < BirthDate.DayOfYear ? 1 : 0);
    
    /// <summary>
    /// Nome completo
    /// </summary>
    public string FullName => $"{LastName} {FirstName}";
}
