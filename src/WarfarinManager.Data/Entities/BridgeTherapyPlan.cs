using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Data.Entities;

/// <summary>
/// Entità piano bridge therapy perioperatoria
/// </summary>
public class BridgeTherapyPlan : BaseEntity
{
    /// <summary>
    /// ID paziente (Foreign Key)
    /// </summary>
    public int PatientId { get; set; }
    
    /// <summary>
    /// Data intervento programmato
    /// </summary>
    public DateTime SurgeryDate { get; set; }
    
    /// <summary>
    /// Tipo chirurgia
    /// </summary>
    public SurgeryType SurgeryType { get; set; }
    
    /// <summary>
    /// Rischio tromboembolico paziente
    /// </summary>
    public ThromboembolicRisk ThromboembolicRisk { get; set; }
    
    /// <summary>
    /// Bridge raccomandato
    /// </summary>
    public bool BridgeRecommended { get; set; }
    
    /// <summary>
    /// Data stop warfarin
    /// </summary>
    public DateTime? WarfarinStopDate { get; set; }
    
    /// <summary>
    /// Data inizio EBPM
    /// </summary>
    public DateTime? EBPMStartDate { get; set; }
    
    /// <summary>
    /// Data ultima dose EBPM pre-operatoria
    /// </summary>
    public DateTime? EBPMLastDoseDate { get; set; }
    
    /// <summary>
    /// Data ripresa warfarin post-operatoria
    /// </summary>
    public DateTime? WarfarinResumeDate { get; set; }
    
    /// <summary>
    /// Data ripresa EBPM post-operatoria
    /// </summary>
    public DateTime? EBPMResumeDate { get; set; }
    
    /// <summary>
    /// Testo protocollo completo
    /// </summary>
    public string? ProtocolText { get; set; }
    
    /// <summary>
    /// Note cliniche
    /// </summary>
    public string? Notes { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// Paziente associato
    /// </summary>
    public Patient Patient { get; set; } = null!;
}
