namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Tipo di chirurgia programmata per bridge therapy
/// </summary>
public enum SurgeryType
{
    // ===== BASSO RISCHIO EMORRAGICO (0-2%) =====
    
    /// <summary>
    /// Endoscopia diagnostica (GI, bronchiale) senza biopsie
    /// </summary>
    DiagnosticEndoscopy,
    
    /// <summary>
    /// Colonscopia diagnostica senza polipectomia
    /// </summary>
    DiagnosticColonoscopy,
    
    /// <summary>
    /// Cateterismo cardiaco diagnostico (destro/sinistro)
    /// </summary>
    CardiacCatheterization,
    
    /// <summary>
    /// Ecocardiografia transesofagea diagnostica
    /// </summary>
    Transesophageal,
    
    /// <summary>
    /// Dermatologia: escissione minore <2-3 cm, superficiale
    /// </summary>
    DermatologySingleExcision,
    
    /// <summary>
    /// Oculistica: laser refrattiva, cataratta
    /// </summary>
    Ophthalmology,
    
    /// <summary>
    /// Odontoiatria: estrazione singola dente, devitalizzazione
    /// </summary>
    DentalSingleExtraction,
    
    // ===== MODERATO RISCHIO EMORRAGICO (2-5%) =====
    
    /// <summary>
    /// Endoscopia diagnostica con biopsia (stomaco, duodeno)
    /// </summary>
    EndoscopyWithBiopsy,
    
    /// <summary>
    /// Polipectomia colonscopia (<10 mm polyps)
    /// </summary>
    Polypectomy,
    
    /// <summary>
    /// Cardioversione elettrica pianificata
    /// </summary>
    Cardioversion,
    
    /// <summary>
    /// Impianto pacemaker/ICD (generatore + lead)
    /// </summary>
    PacemakerImplant,
    
    /// <summary>
    /// Chirurgia ortopedica minore (artroscopia, infiltrativa)
    /// </summary>
    ArthroscopyMinor,
    
    /// <summary>
    /// Colecistectomia laparoscopica
    /// </summary>
    LaparoscopicCholecystectomy,
    
    /// <summary>
    /// TURP, cistoscopia con biopsia
    /// </summary>
    TURP,
    
    // ===== ALTO RISCHIO EMORRAGICO (>5%) =====
    
    /// <summary>
    /// Neurochirurgia (spinale, cerebrale, plexus)
    /// </summary>
    Neurosurgery,
    
    /// <summary>
    /// Chirurgia cardiaca aperta (bypass coronarico)
    /// </summary>
    CardiacSurgery,
    
    /// <summary>
    /// Chirurgia vascolare maggiore (aneurisma, endarterectomia)
    /// </summary>
    VascularSurgery,
    
    /// <summary>
    /// Chirurgia polmonare (lobectomia)
    /// </summary>
    ThoracicSurgery,
    
    /// <summary>
    /// Chirurgia gastrointestinale (colectomia, gastrectomia)
    /// </summary>
    AbdominalSurgery,
    
    /// <summary>
    /// Chirurgia epatica (resezione)
    /// </summary>
    HepaticSurgery,
    
    /// <summary>
    /// Chirurgia pancreatica (Whipple)
    /// </summary>
    PancreaticSurgery,
    
    /// <summary>
    /// Chirurgia prostatica (TURP, prostatectomia radicale)
    /// </summary>
    ProstateSurgery,
    
    /// <summary>
    /// Chirurgia renale (nefrectomia)
    /// </summary>
    RenalSurgery,
    
    /// <summary>
    /// Chirurgia ortopedica maggiore (protesi articolari, fratture complesse)
    /// </summary>
    MajorOrthopedic,
    
    /// <summary>
    /// Anestesia neuroassiale (spinale, epidurale) - rischio ematoma epidurale
    /// </summary>
    EpiduralAnesthesia,
    
    /// <summary>
    /// Oftalmochiurgia (retina, glaucoma)
    /// </summary>
    OphthalmologySurgery,
    
    /// <summary>
    /// Odontoiatria maggiore (≥3 estrazioni)
    /// </summary>
    DentalMajor,
    
    /// <summary>
    /// Altra procedura non categorizzata
    /// </summary>
    Other
}
