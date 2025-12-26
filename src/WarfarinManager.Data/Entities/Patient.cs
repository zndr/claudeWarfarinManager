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

    /// <summary>
    /// Flag per paziente naive (inizia la fase di induzione della terapia)
    /// </summary>
    public bool IsNaive { get; set; }

    /// <summary>
    /// Flag che indica se il wizard iniziale obbligatorio è stato completato
    /// (Indicazione, Valutazione Pre-TAO, CHA2DS2-VASc, HAS-BLED)
    /// </summary>
    public bool IsInitialWizardCompleted { get; set; }

    /// <summary>
    /// Tipo di anticoagulante in uso (warfarin, dabigatran, rivaroxaban, apixaban, edoxaban, altro)
    /// </summary>
    public string? AnticoagulantType { get; set; }

    /// <summary>
    /// Data di inizio della terapia anticoagulante
    /// </summary>
    public DateTime? TherapyStartDate { get; set; }

    /// <summary>
    /// Flag per soft delete - se true il paziente è "eliminato" ma i dati restano nel DB
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Data di eliminazione (soft delete)
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    
    // CHA₂DS₂-VASc Score Components (per calcolo rischio bridge therapy)
    
    /// <summary>
    /// Presenza di scompenso cardiaco congestizio / disfunzione VS
    /// </summary>
    public bool HasCongestiveHeartFailure { get; set; }
    
    /// <summary>
    /// Ipertensione arteriosa (in trattamento)
    /// </summary>
    public bool HasHypertension { get; set; }
    
    /// <summary>
    /// Diabete mellito (in trattamento)
    /// </summary>
    public bool HasDiabetes { get; set; }
    
    /// <summary>
    /// Presenza di malattia vascolare (pregresso IMA, PAD, placca aortica)
    /// </summary>
    public bool HasVascularDisease { get; set; }

    // HAS-BLED Score Components (per calcolo rischio emorragico)

    /// <summary>
    /// Funzione renale compromessa (dialisi, trapianto, creatinina >200 μmol/L)
    /// </summary>
    public bool HasRenalDisease { get; set; }

    /// <summary>
    /// Funzione epatica compromessa (cirrosi, bili >2x, AST/ALT/ALP >3x)
    /// </summary>
    public bool HasLiverDisease { get; set; }

    /// <summary>
    /// Storia di stroke
    /// </summary>
    public bool HasStroke { get; set; }

    /// <summary>
    /// Storia di sanguinamento o predisposizione al sanguinamento
    /// </summary>
    public bool HasBleedingHistory { get; set; }

    /// <summary>
    /// INR labile (TTR <60%)
    /// </summary>
    public bool HasLabileINR { get; set; }

    /// <summary>
    /// Uso di farmaci che predispongono al sanguinamento (antiaggreganti, FANS)
    /// </summary>
    public bool UsesDrugsIncreasingBleedingRisk { get; set; }

    /// <summary>
    /// Uso di alcol (≥8 unità/settimana)
    /// </summary>
    public bool UsesAlcohol { get; set; }

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

    /// <summary>
    /// Valutazioni pre-TAO
    /// </summary>
    public ICollection<PreTaoAssessment> PreTaoAssessments { get; set; } = new List<PreTaoAssessment>();

    /// <summary>
    /// Record di monitoraggio DOAC
    /// </summary>
    public ICollection<DoacMonitoringRecord> DoacMonitoringRecords { get; set; } = new List<DoacMonitoringRecord>();

    /// <summary>
    /// Terapie continuative
    /// </summary>
    public ICollection<TerapiaContinuativa> TerapieContinuative { get; set; } = new List<TerapiaContinuativa>();

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

    /// <summary>
    /// Calcola il punteggio HAS-BLED per valutare il rischio emorragico
    /// H: Hypertension (1 punto)
    /// A: Abnormal renal/liver function (1 punto ciascuno, max 2)
    /// S: Stroke (1 punto)
    /// B: Bleeding (1 punto)
    /// L: Labile INR (1 punto)
    /// E: Elderly (>65 anni, 1 punto)
    /// D: Drugs/alcohol (1 punto ciascuno, max 2)
    /// Range: 0-9 punti. ≥3 indica alto rischio emorragico
    /// </summary>
    public int HasBledScore
    {
        get
        {
            int score = 0;

            // H: Hypertension
            if (HasHypertension) score++;

            // A: Abnormal renal/liver function (max 2)
            if (HasRenalDisease) score++;
            if (HasLiverDisease) score++;

            // S: Stroke
            if (HasStroke) score++;

            // B: Bleeding history
            if (HasBleedingHistory) score++;

            // L: Labile INR
            if (HasLabileINR) score++;

            // E: Elderly (>65 anni)
            if (Age > 65) score++;

            // D: Drugs/alcohol (max 2)
            if (UsesDrugsIncreasingBleedingRisk) score++;
            if (UsesAlcohol) score++;

            return score;
        }
    }
}
