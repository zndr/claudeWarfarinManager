using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Shared.Models;

/// <summary>
/// Protocollo di gestione perioperatoria per DOAC
/// </summary>
public class DOACPerioperativeProtocol
{
    /// <summary>
    /// Tipo di DOAC
    /// </summary>
    public DOACType DOACType { get; set; }

    /// <summary>
    /// Funzione renale del paziente
    /// </summary>
    public RenalFunction RenalFunction { get; set; }

    /// <summary>
    /// eGFR mL/min
    /// </summary>
    public double EGFR { get; set; }

    /// <summary>
    /// Rischio emorragico procedurale
    /// </summary>
    public BleedingRisk BleedingRisk { get; set; }

    /// <summary>
    /// Rischio trombotico paziente
    /// </summary>
    public ThromboembolicRisk ThromboembolicRisk { get; set; }

    /// <summary>
    /// Data intervento
    /// </summary>
    public DateTime SurgeryDate { get; set; }

    /// <summary>
    /// Tipo di chirurgia
    /// </summary>
    public SurgeryType SurgeryType { get; set; }

    // Timing pre-operatorio

    /// <summary>
    /// Ore di sospensione pre-operatoria raccomandate
    /// </summary>
    public int PreOpSuspensionHours { get; set; }

    /// <summary>
    /// Data/ora ultima dose DOAC
    /// </summary>
    public DateTime LastDOACDose { get; set; }

    /// <summary>
    /// Descrizione timing sospensione (es. "48h (giorno -3)")
    /// </summary>
    public string PreOpTimingDescription { get; set; } = string.Empty;

    // Timing post-operatorio

    /// <summary>
    /// Ore minime post-operatorie prima della ripresa
    /// </summary>
    public int PostOpMinHours { get; set; }

    /// <summary>
    /// Ore massime post-operatorie (range)
    /// </summary>
    public int PostOpMaxHours { get; set; }

    /// <summary>
    /// Data/ora ripresa DOAC
    /// </summary>
    public DateTime DOACResumeDate { get; set; }

    /// <summary>
    /// Descrizione timing ripresa
    /// </summary>
    public string PostOpTimingDescription { get; set; } = string.Empty;

    // Bridging

    /// <summary>
    /// Se true, il bridging NON è raccomandato (principio DOAC)
    /// </summary>
    public bool NoBridgingRecommended { get; set; } = true;

    /// <summary>
    /// Se necessario bridging per alto rischio trombotico
    /// </summary>
    public bool BridgingRequired { get; set; }

    /// <summary>
    /// Tipo di EBPM per bridging (se necessario)
    /// </summary>
    public string? EBPMType { get; set; }

    /// <summary>
    /// Dosaggio EBPM
    /// </summary>
    public string? EBPMDosage { get; set; }

    // Raccomandazioni

    /// <summary>
    /// Raccomandazione principale
    /// </summary>
    public string MainRecommendation { get; set; } = string.Empty;

    /// <summary>
    /// Avvertenze specifiche
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Note cliniche
    /// </summary>
    public List<string> ClinicalNotes { get; set; } = new();

    /// <summary>
    /// Se true, raccomandato dosaggio livelli plasmatici DOAC pre-operatori
    /// </summary>
    public bool RecommendPlasmaLevelTesting { get; set; }

    /// <summary>
    /// Target livello plasmatico DOAC (ng/mL)
    /// </summary>
    public int? TargetPlasmaLevel { get; set; }
}
