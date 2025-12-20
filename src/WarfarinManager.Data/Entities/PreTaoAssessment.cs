namespace WarfarinManager.Data.Entities;

/// <summary>
/// Valutazione pre-TAO per stratificazione rischio trombotico ed emorragico
/// </summary>
public class PreTaoAssessment : BaseEntity
{
    /// <summary>
    /// ID del paziente (FK)
    /// </summary>
    public int PatientId { get; set; }

    /// <summary>
    /// Data della valutazione
    /// </summary>
    public DateTime AssessmentDate { get; set; } = DateTime.Now;

    // ==================== CHA₂DS₂-VASc Score Components ====================

    /// <summary>
    /// Scompenso cardiaco / disfunzione VS (1 punto)
    /// </summary>
    public bool CongestiveHeartFailure { get; set; }

    /// <summary>
    /// Ipertensione arteriosa (1 punto)
    /// </summary>
    public bool Hypertension { get; set; }

    /// <summary>
    /// Età ≥75 anni (2 punti)
    /// </summary>
    public bool Age75OrMore { get; set; }

    /// <summary>
    /// Diabete mellito (1 punto)
    /// </summary>
    public bool Diabetes { get; set; }

    /// <summary>
    /// Stroke/TIA/TE pregresso (2 punti)
    /// </summary>
    public bool PriorStrokeTiaTE { get; set; }

    /// <summary>
    /// Malattia vascolare (pregresso IMA, PAD, placca aortica) (1 punto)
    /// </summary>
    public bool VascularDisease { get; set; }

    /// <summary>
    /// Età 65-74 anni (1 punto)
    /// </summary>
    public bool Age65To74 { get; set; }

    /// <summary>
    /// Sesso femminile (1 punto)
    /// </summary>
    public bool Female { get; set; }

    // ==================== HAS-BLED Score Components ====================

    /// <summary>
    /// H - Hypertension: PAS >160 mmHg (1 punto)
    /// </summary>
    public bool HasBledHypertension { get; set; }

    /// <summary>
    /// A - Abnormal renal function: Dialisi, trapianto, Cr >2.26 mg/dL o >200 μmol/L (1 punto)
    /// </summary>
    public bool AbnormalRenalFunction { get; set; }

    /// <summary>
    /// A - Abnormal liver function: Cirrosi o bilirubina >2x, AST/ALT/ALP >3x (1 punto)
    /// </summary>
    public bool AbnormalLiverFunction { get; set; }

    /// <summary>
    /// S - Stroke: Storia di stroke (1 punto)
    /// </summary>
    public bool StrokeHistory { get; set; }

    /// <summary>
    /// B - Bleeding: Storia di sanguinamento maggiore o predisposizione (1 punto)
    /// </summary>
    public bool BleedingHistory { get; set; }

    /// <summary>
    /// L - Labile INR: TTR <60% (se già in TAO) (1 punto)
    /// </summary>
    public bool LabileINR { get; set; }

    /// <summary>
    /// E - Elderly: Età >65 anni (1 punto)
    /// </summary>
    public bool Elderly { get; set; }

    /// <summary>
    /// D - Drugs: Farmaci concomitanti (antipiastrinici, FANS) (1 punto)
    /// </summary>
    public bool DrugsPredisposing { get; set; }

    /// <summary>
    /// D - Drugs/Alcohol: Abuso di alcol ≥8 drink/settimana (1 punto)
    /// </summary>
    public bool AlcoholAbuse { get; set; }

    // ==================== Controindicazioni Assolute ====================

    /// <summary>
    /// Emorragia maggiore in atto o recente (<3 mesi)
    /// </summary>
    public bool ActiveMajorBleeding { get; set; }

    /// <summary>
    /// Gravidanza in corso
    /// </summary>
    public bool Pregnancy { get; set; }

    /// <summary>
    /// Discrasia ematica severa (grave trombocitopenia <50.000/μL, coagulopatia)
    /// </summary>
    public bool SevereBloodDyscrasia { get; set; }

    /// <summary>
    /// Recente intervento neurochirurgico o oculare (<1 mese)
    /// </summary>
    public bool RecentNeurosurgery { get; set; }

    /// <summary>
    /// Emorragia intracranica recente o malformazione vascolare cerebrale
    /// </summary>
    public bool IntracranialBleedingOrMalformation { get; set; }

    /// <summary>
    /// Ulcera peptica attiva o varici esofagee a rischio
    /// </summary>
    public bool ActivePepticUlcerOrVarices { get; set; }

    /// <summary>
    /// Endocardite batterica acuta
    /// </summary>
    public bool AcuteBacterialEndocarditis { get; set; }

    /// <summary>
    /// Ipertensione severa non controllata (PAS >200 mmHg, PAD >120 mmHg)
    /// </summary>
    public bool SevereUncontrolledHypertension { get; set; }

    /// <summary>
    /// Allergia documentata al warfarin
    /// </summary>
    public bool WarfarinAllergy { get; set; }

    /// <summary>
    /// Mancanza di compliance/supervisione
    /// </summary>
    public bool LackOfCompliance { get; set; }

    // ==================== Controindicazioni Relative ====================

    /// <summary>
    /// Recente emorragia gastrointestinale (3-6 mesi)
    /// </summary>
    public bool RecentGIBleeding { get; set; }

    /// <summary>
    /// Storia di sanguinamenti maggiori (>6 mesi)
    /// </summary>
    public bool HistoryOfMajorBleeding { get; set; }

    /// <summary>
    /// Insufficienza renale moderata (GFR 30-60 mL/min)
    /// </summary>
    public bool ModerateRenalFailure { get; set; }

    /// <summary>
    /// Insufficienza epatica moderata (Child-Pugh B)
    /// </summary>
    public bool ModerateHepaticFailure { get; set; }

    /// <summary>
    /// Piastrinopenia moderata (50.000-100.000/μL)
    /// </summary>
    public bool ModerateThrombocytopenia { get; set; }

    /// <summary>
    /// Cadute frequenti / rischio traumatico elevato
    /// </summary>
    public bool FrequentFalls { get; set; }

    /// <summary>
    /// Demenza o deficit cognitivo
    /// </summary>
    public bool CognitiveImpairment { get; set; }

    /// <summary>
    /// Recente chirurgia maggiore (1-3 mesi)
    /// </summary>
    public bool RecentMajorSurgery { get; set; }

    /// <summary>
    /// Lesioni organiche a rischio (aneurismi, neoplasie)
    /// </summary>
    public bool OrganicLesionsAtRisk { get; set; }

    /// <summary>
    /// Pericardite acuta
    /// </summary>
    public bool AcutePericarditis { get; set; }

    // ==================== Fattori Favorenti Eventi Avversi ====================

    /// <summary>
    /// Politerapia (>5 farmaci concomitanti)
    /// </summary>
    public bool Polypharmacy { get; set; }

    /// <summary>
    /// Isolamento sociale / difficoltà di accesso al follow-up
    /// </summary>
    public bool SocialIsolation { get; set; }

    /// <summary>
    /// Interazioni farmacologiche note
    /// </summary>
    public bool KnownDrugInteractions { get; set; }

    /// <summary>
    /// Dieta irregolare o ricca di vitamina K
    /// </summary>
    public bool IrregularDietOrHighVitaminK { get; set; }

    /// <summary>
    /// IMC estremo (<18 o >35 kg/m²)
    /// </summary>
    public bool ExtremeBMI { get; set; }

    /// <summary>
    /// Anemia cronica (Hb <10 g/dL)
    /// </summary>
    public bool ChronicAnemia { get; set; }

    /// <summary>
    /// Patologie oncologiche attive
    /// </summary>
    public bool ActiveCancer { get; set; }

    /// <summary>
    /// Procedura invasiva programmata entro 3 mesi
    /// </summary>
    public bool ScheduledInvasiveProcedure { get; set; }

    /// <summary>
    /// Varianti genetiche note (CYP2C9, VKORC1)
    /// </summary>
    public bool KnownGeneticVariants { get; set; }

    // ==================== Note e Valutazione Clinica ====================

    /// <summary>
    /// Note cliniche libere sulla valutazione
    /// </summary>
    public string? ClinicalNotes { get; set; }

    /// <summary>
    /// Conclusioni e raccomandazioni
    /// </summary>
    public string? Recommendations { get; set; }

    /// <summary>
    /// Valutazione approvata (pronto per inizio TAO)
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// Data approvazione
    /// </summary>
    public DateTime? ApprovalDate { get; set; }

    /// <summary>
    /// Medico che ha effettuato la valutazione
    /// </summary>
    public string? AssessingPhysician { get; set; }

    // Navigation Properties

    /// <summary>
    /// Paziente valutato
    /// </summary>
    public Patient Patient { get; set; } = null!;

    // ==================== Computed Properties ====================

    /// <summary>
    /// Calcolo automatico CHA₂DS₂-VASc Score (0-9 punti)
    /// </summary>
    public int CHA2DS2VAScScore =>
        (CongestiveHeartFailure ? 1 : 0) +
        (Hypertension ? 1 : 0) +
        (Age75OrMore ? 2 : 0) +
        (Diabetes ? 1 : 0) +
        (PriorStrokeTiaTE ? 2 : 0) +
        (VascularDisease ? 1 : 0) +
        (Age65To74 ? 1 : 0) +
        (Female ? 1 : 0);

    /// <summary>
    /// Calcolo automatico HAS-BLED Score (0-9 punti)
    /// </summary>
    public int HASBLEDScore =>
        (HasBledHypertension ? 1 : 0) +
        (AbnormalRenalFunction ? 1 : 0) +
        (AbnormalLiverFunction ? 1 : 0) +
        (StrokeHistory ? 1 : 0) +
        (BleedingHistory ? 1 : 0) +
        (LabileINR ? 1 : 0) +
        (Elderly ? 1 : 0) +
        (DrugsPredisposing ? 1 : 0) +
        (AlcoholAbuse ? 1 : 0);

    /// <summary>
    /// Interpretazione rischio tromboembolico
    /// </summary>
    public string ThromboticRiskLevel => CHA2DS2VAScScore switch
    {
        0 => "Basso (0 punti)",
        1 => "Basso-Moderato (1 punto)",
        >= 2 => $"Alto ({CHA2DS2VAScScore} punti)",
        _ => "Non valutato"
    };

    /// <summary>
    /// Interpretazione rischio emorragico
    /// </summary>
    public string BleedingRiskLevel => HASBLEDScore switch
    {
        0 => "Basso (0 punti)",
        1 or 2 => $"Moderato ({HASBLEDScore} punti)",
        >= 3 => $"Alto ({HASBLEDScore} punti)",
        _ => "Non valutato"
    };

    /// <summary>
    /// Verifica presenza di controindicazioni assolute
    /// </summary>
    public bool HasAbsoluteContraindications =>
        ActiveMajorBleeding ||
        Pregnancy ||
        SevereBloodDyscrasia ||
        RecentNeurosurgery ||
        IntracranialBleedingOrMalformation ||
        ActivePepticUlcerOrVarices ||
        AcuteBacterialEndocarditis ||
        SevereUncontrolledHypertension ||
        WarfarinAllergy ||
        LackOfCompliance;

    /// <summary>
    /// Numero di controindicazioni relative presenti
    /// </summary>
    public int RelativeContraindicationsCount =>
        (RecentGIBleeding ? 1 : 0) +
        (HistoryOfMajorBleeding ? 1 : 0) +
        (ModerateRenalFailure ? 1 : 0) +
        (ModerateHepaticFailure ? 1 : 0) +
        (ModerateThrombocytopenia ? 1 : 0) +
        (FrequentFalls ? 1 : 0) +
        (CognitiveImpairment ? 1 : 0) +
        (RecentMajorSurgery ? 1 : 0) +
        (OrganicLesionsAtRisk ? 1 : 0) +
        (AcutePericarditis ? 1 : 0);

    /// <summary>
    /// Numero di fattori favorenti eventi avversi
    /// </summary>
    public int AdverseEventRiskFactorsCount =>
        (Polypharmacy ? 1 : 0) +
        (SocialIsolation ? 1 : 0) +
        (KnownDrugInteractions ? 1 : 0) +
        (IrregularDietOrHighVitaminK ? 1 : 0) +
        (ExtremeBMI ? 1 : 0) +
        (ChronicAnemia ? 1 : 0) +
        (ActiveCancer ? 1 : 0) +
        (ScheduledInvasiveProcedure ? 1 : 0) +
        (KnownGeneticVariants ? 1 : 0);

    /// <summary>
    /// Valutazione globale idoneità alla TAO
    /// </summary>
    public string OverallAssessment
    {
        get
        {
            if (HasAbsoluteContraindications)
                return "❌ CONTROINDICATO - Presenza di controindicazioni assolute";

            if (HASBLEDScore >= CHA2DS2VAScScore && HASBLEDScore >= 3)
                return "⚠️ ATTENZIONE - Rischio emorragico superiore/uguale a rischio trombotico";

            if (RelativeContraindicationsCount >= 3)
                return "⚠️ ATTENZIONE - Presenza di multiple controindicazioni relative";

            if (CHA2DS2VAScScore >= 2 && HASBLEDScore <= 2)
                return "✓ INDICATO - Chiaro beneficio dalla TAO";

            if (CHA2DS2VAScScore == 1)
                return "⚖️ DA VALUTARE - Considerare TAO vs altri anticoagulanti";

            if (CHA2DS2VAScScore == 0)
                return "⚖️ NON INDICATO - Rischio tromboembolico basso";

            return "⚖️ DA VALUTARE - Valutazione rischio/beneficio individualizzata";
        }
    }
}
