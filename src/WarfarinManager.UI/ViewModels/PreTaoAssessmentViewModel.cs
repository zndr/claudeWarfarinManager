using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.Shared.Enums;
using WarfarinManager.UI.Services;
using WarfarinManager.UI.Views.Patient;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per valutazione pre-TAO (stratificazione rischio)
/// </summary>
public partial class PreTaoAssessmentViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDialogService _dialogService;
    private readonly ILogger<PreTaoAssessmentViewModel> _logger;
    private int _patientId;
    private Patient? _patient;

    // ==================== CHA₂DS₂-VASc Components ====================

    [ObservableProperty]
    private bool _congestiveHeartFailure;

    [ObservableProperty]
    private bool _hypertension;

    [ObservableProperty]
    private bool _age75OrMore;

    [ObservableProperty]
    private bool _diabetes;

    [ObservableProperty]
    private bool _priorStrokeTiaTE;

    [ObservableProperty]
    private bool _vascularDisease;

    [ObservableProperty]
    private bool _age65To74;

    [ObservableProperty]
    private bool _female;

    // ==================== HAS-BLED Components ====================

    [ObservableProperty]
    private bool _hasBledHypertension;

    [ObservableProperty]
    private bool _abnormalRenalFunction;

    [ObservableProperty]
    private bool _abnormalLiverFunction;

    [ObservableProperty]
    private bool _strokeHistory;

    [ObservableProperty]
    private bool _bleedingHistory;

    [ObservableProperty]
    private bool _labileINR;

    [ObservableProperty]
    private bool _elderly;

    [ObservableProperty]
    private bool _drugsPredisposing;

    [ObservableProperty]
    private bool _alcoholAbuse;

    // ==================== Controindicazioni Assolute ====================

    [ObservableProperty]
    private bool _activeMajorBleeding;

    [ObservableProperty]
    private bool _pregnancy;

    [ObservableProperty]
    private bool _severeBloodDyscrasia;

    [ObservableProperty]
    private bool _recentNeurosurgery;

    [ObservableProperty]
    private bool _intracranialBleedingOrMalformation;

    [ObservableProperty]
    private bool _activePepticUlcerOrVarices;

    [ObservableProperty]
    private bool _acuteBacterialEndocarditis;

    [ObservableProperty]
    private bool _severeUncontrolledHypertension;

    [ObservableProperty]
    private bool _warfarinAllergy;

    [ObservableProperty]
    private bool _lackOfCompliance;

    // ==================== Controindicazioni Relative ====================

    [ObservableProperty]
    private bool _recentGIBleeding;

    [ObservableProperty]
    private bool _historyOfMajorBleeding;

    [ObservableProperty]
    private bool _moderateRenalFailure;

    [ObservableProperty]
    private bool _moderateHepaticFailure;

    [ObservableProperty]
    private bool _moderateThrombocytopenia;

    [ObservableProperty]
    private bool _frequentFalls;

    [ObservableProperty]
    private bool _cognitiveImpairment;

    [ObservableProperty]
    private bool _recentMajorSurgery;

    [ObservableProperty]
    private bool _organicLesionsAtRisk;

    [ObservableProperty]
    private bool _acutePericarditis;

    // ==================== Fattori Favorenti Eventi Avversi ====================

    [ObservableProperty]
    private bool _polypharmacy;

    [ObservableProperty]
    private bool _socialIsolation;

    [ObservableProperty]
    private bool _knownDrugInteractions;

    [ObservableProperty]
    private bool _irregularDietOrHighVitaminK;

    [ObservableProperty]
    private bool _extremeBMI;

    [ObservableProperty]
    private bool _chronicAnemia;

    [ObservableProperty]
    private bool _activeCancer;

    [ObservableProperty]
    private bool _scheduledInvasiveProcedure;

    [ObservableProperty]
    private bool _knownGeneticVariants;

    // ==================== Note e Valutazione ====================

    [ObservableProperty]
    private string _clinicalNotes = string.Empty;

    [ObservableProperty]
    private string _recommendations = string.Empty;

    [ObservableProperty]
    private string _assessingPhysician = string.Empty;

    [ObservableProperty]
    private bool _isApproved;

    [ObservableProperty]
    private bool _isSaving;

    /// <summary>
    /// Indica se il form è utilizzato all'interno del wizard (disabilita messaggi di conferma)
    /// </summary>
    [ObservableProperty]
    private bool _isWizardMode;

    // ==================== Computed Properties ====================

    public int CHA2DS2VAScScore =>
        (CongestiveHeartFailure ? 1 : 0) +
        (Hypertension ? 1 : 0) +
        (Age75OrMore ? 2 : 0) +
        (Diabetes ? 1 : 0) +
        (PriorStrokeTiaTE ? 2 : 0) +
        (VascularDisease ? 1 : 0) +
        (Age65To74 ? 1 : 0) +
        (Female ? 1 : 0);

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

    public string ThromboticRiskLevel => CHA2DS2VAScScore switch
    {
        0 => "Basso (0 punti)",
        1 => "Basso-Moderato (1 punto)",
        >= 2 => $"Alto ({CHA2DS2VAScScore} punti)",
        _ => "Non valutato"
    };

    public string BleedingRiskLevel => HASBLEDScore switch
    {
        0 => "Basso (0 punti)",
        1 or 2 => $"Moderato ({HASBLEDScore} punti)",
        >= 3 => $"Alto ({HASBLEDScore} punti)",
        _ => "Non valutato"
    };

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

    public string OverallAssessment
    {
        get
        {
            if (HasAbsoluteContraindications)
                return "CONTROINDICATO - Presenza di controindicazioni assolute";

            if (HASBLEDScore >= CHA2DS2VAScScore && HASBLEDScore >= 3)
                return "ATTENZIONE - Rischio emorragico superiore/uguale a rischio trombotico";

            if (RelativeContraindicationsCount >= 3)
                return "ATTENZIONE - Presenza di multiple controindicazioni relative";

            if (CHA2DS2VAScScore >= 2 && HASBLEDScore <= 2)
                return "INDICATO - Chiaro beneficio dalla TAO";

            if (CHA2DS2VAScScore == 1)
                return "DA VALUTARE - Considerare TAO vs altri anticoagulanti";

            if (CHA2DS2VAScScore == 0)
                return "NON INDICATO - Rischio tromboembolico basso";

            return "DA VALUTARE - Valutazione rischio/beneficio individualizzata";
        }
    }

    public PreTaoAssessmentViewModel(
        IUnitOfWork unitOfWork,
        IDialogService dialogService,
        ILogger<PreTaoAssessmentViewModel> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Inizializza il ViewModel con i dati del paziente
    /// </summary>
    public async Task InitializeAsync(int patientId)
    {
        _patientId = patientId;
        _patient = await _unitOfWork.Patients.GetByIdAsync(patientId);

        if (_patient == null)
        {
            _dialogService.ShowError("Paziente non trovato", "Errore");
            return;
        }

        // Auto-popola i campi in base all'età e sesso del paziente
        AutoPopulateFromPatient();

        // Carica eventuale valutazione precedente
        await LoadExistingAssessmentAsync();
    }

    /// <summary>
    /// Auto-popola campi in base ai dati del paziente
    /// </summary>
    private void AutoPopulateFromPatient()
    {
        if (_patient == null) return;

        // Età
        var age = _patient.Age;
        Age75OrMore = age >= 75;
        Age65To74 = age >= 65 && age < 75;
        Elderly = age > 65;

        // Sesso
        Female = _patient.Gender == Gender.Female;

        // Dati già presenti nel paziente
        CongestiveHeartFailure = _patient.HasCongestiveHeartFailure;
        Hypertension = _patient.HasHypertension;
        Diabetes = _patient.HasDiabetes;
        VascularDisease = _patient.HasVascularDisease;

        OnPropertyChanged(nameof(CHA2DS2VAScScore));
        OnPropertyChanged(nameof(HASBLEDScore));
        OnPropertyChanged(nameof(ThromboticRiskLevel));
        OnPropertyChanged(nameof(BleedingRiskLevel));
        OnPropertyChanged(nameof(OverallAssessment));
    }

    /// <summary>
    /// Carica eventuale valutazione pre-TAO esistente
    /// </summary>
    private async Task LoadExistingAssessmentAsync()
    {
        var latestAssessment = await _unitOfWork.Database.PreTaoAssessments
            .Where(a => a.PatientId == _patientId)
            .OrderByDescending(a => a.AssessmentDate)
            .FirstOrDefaultAsync();

        if (latestAssessment != null)
        {
            LoadFromEntity(latestAssessment);
        }
        else
        {
            // Se non c'è valutazione precedente, precompila il nome del medico
            await LoadDoctorNameAsync();
        }
    }

    /// <summary>
    /// Carica il nome del medico corrente
    /// </summary>
    private async Task LoadDoctorNameAsync()
    {
        try
        {
            var doctorData = await _unitOfWork.Database.DoctorData.FirstOrDefaultAsync();
            if (doctorData != null && !string.IsNullOrWhiteSpace(doctorData.FullName))
            {
                AssessingPhysician = doctorData.FullName;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossibile caricare il nome del medico");
            // Non è un errore critico, il campo può essere compilato manualmente
        }
    }

    /// <summary>
    /// Carica dati da entità esistente
    /// </summary>
    private void LoadFromEntity(PreTaoAssessment assessment)
    {
        // CHA2DS2VASc
        CongestiveHeartFailure = assessment.CongestiveHeartFailure;
        Hypertension = assessment.Hypertension;
        Age75OrMore = assessment.Age75OrMore;
        Diabetes = assessment.Diabetes;
        PriorStrokeTiaTE = assessment.PriorStrokeTiaTE;
        VascularDisease = assessment.VascularDisease;
        Age65To74 = assessment.Age65To74;
        Female = assessment.Female;

        // HAS-BLED
        HasBledHypertension = assessment.HasBledHypertension;
        AbnormalRenalFunction = assessment.AbnormalRenalFunction;
        AbnormalLiverFunction = assessment.AbnormalLiverFunction;
        StrokeHistory = assessment.StrokeHistory;
        BleedingHistory = assessment.BleedingHistory;
        LabileINR = assessment.LabileINR;
        Elderly = assessment.Elderly;
        DrugsPredisposing = assessment.DrugsPredisposing;
        AlcoholAbuse = assessment.AlcoholAbuse;

        // Controindicazioni assolute
        ActiveMajorBleeding = assessment.ActiveMajorBleeding;
        Pregnancy = assessment.Pregnancy;
        SevereBloodDyscrasia = assessment.SevereBloodDyscrasia;
        RecentNeurosurgery = assessment.RecentNeurosurgery;
        IntracranialBleedingOrMalformation = assessment.IntracranialBleedingOrMalformation;
        ActivePepticUlcerOrVarices = assessment.ActivePepticUlcerOrVarices;
        AcuteBacterialEndocarditis = assessment.AcuteBacterialEndocarditis;
        SevereUncontrolledHypertension = assessment.SevereUncontrolledHypertension;
        WarfarinAllergy = assessment.WarfarinAllergy;
        LackOfCompliance = assessment.LackOfCompliance;

        // Controindicazioni relative
        RecentGIBleeding = assessment.RecentGIBleeding;
        HistoryOfMajorBleeding = assessment.HistoryOfMajorBleeding;
        ModerateRenalFailure = assessment.ModerateRenalFailure;
        ModerateHepaticFailure = assessment.ModerateHepaticFailure;
        ModerateThrombocytopenia = assessment.ModerateThrombocytopenia;
        FrequentFalls = assessment.FrequentFalls;
        CognitiveImpairment = assessment.CognitiveImpairment;
        RecentMajorSurgery = assessment.RecentMajorSurgery;
        OrganicLesionsAtRisk = assessment.OrganicLesionsAtRisk;
        AcutePericarditis = assessment.AcutePericarditis;

        // Fattori favorenti
        Polypharmacy = assessment.Polypharmacy;
        SocialIsolation = assessment.SocialIsolation;
        KnownDrugInteractions = assessment.KnownDrugInteractions;
        IrregularDietOrHighVitaminK = assessment.IrregularDietOrHighVitaminK;
        ExtremeBMI = assessment.ExtremeBMI;
        ChronicAnemia = assessment.ChronicAnemia;
        ActiveCancer = assessment.ActiveCancer;
        ScheduledInvasiveProcedure = assessment.ScheduledInvasiveProcedure;
        KnownGeneticVariants = assessment.KnownGeneticVariants;

        // Note
        ClinicalNotes = assessment.ClinicalNotes ?? string.Empty;
        Recommendations = assessment.Recommendations ?? string.Empty;
        AssessingPhysician = assessment.AssessingPhysician ?? string.Empty;
        IsApproved = assessment.IsApproved;

        UpdateScores();
    }

    /// <summary>
    /// Aggiorna tutti i punteggi calcolati
    /// </summary>
    private void UpdateScores()
    {
        OnPropertyChanged(nameof(CHA2DS2VAScScore));
        OnPropertyChanged(nameof(HASBLEDScore));
        OnPropertyChanged(nameof(ThromboticRiskLevel));
        OnPropertyChanged(nameof(BleedingRiskLevel));
        OnPropertyChanged(nameof(HasAbsoluteContraindications));
        OnPropertyChanged(nameof(RelativeContraindicationsCount));
        OnPropertyChanged(nameof(AdverseEventRiskFactorsCount));
        OnPropertyChanged(nameof(OverallAssessment));
    }

    /// <summary>
    /// Notifica cambiamenti per aggiornare gli score
    /// </summary>
    partial void OnCongestiveHeartFailureChanged(bool value) => UpdateScores();
    partial void OnHypertensionChanged(bool value) => UpdateScores();
    partial void OnAge75OrMoreChanged(bool value) => UpdateScores();
    partial void OnDiabetesChanged(bool value) => UpdateScores();
    partial void OnPriorStrokeTiaTEChanged(bool value) => UpdateScores();
    partial void OnVascularDiseaseChanged(bool value) => UpdateScores();
    partial void OnAge65To74Changed(bool value) => UpdateScores();
    partial void OnFemaleChanged(bool value) => UpdateScores();
    partial void OnHasBledHypertensionChanged(bool value) => UpdateScores();
    partial void OnAbnormalRenalFunctionChanged(bool value) => UpdateScores();
    partial void OnAbnormalLiverFunctionChanged(bool value) => UpdateScores();
    partial void OnStrokeHistoryChanged(bool value) => UpdateScores();
    partial void OnBleedingHistoryChanged(bool value) => UpdateScores();
    partial void OnLabileINRChanged(bool value) => UpdateScores();
    partial void OnElderlyChanged(bool value) => UpdateScores();
    partial void OnDrugsPredisposingChanged(bool value) => UpdateScores();
    partial void OnAlcoholAbuseChanged(bool value) => UpdateScores();
    partial void OnActiveMajorBleedingChanged(bool value) => UpdateScores();
    partial void OnPregnancyChanged(bool value) => UpdateScores();
    partial void OnSevereBloodDyscrasiaChanged(bool value) => UpdateScores();
    partial void OnRecentNeurosurgeryChanged(bool value) => UpdateScores();
    partial void OnIntracranialBleedingOrMalformationChanged(bool value) => UpdateScores();
    partial void OnActivePepticUlcerOrVaricesChanged(bool value) => UpdateScores();
    partial void OnAcuteBacterialEndocarditisChanged(bool value) => UpdateScores();
    partial void OnSevereUncontrolledHypertensionChanged(bool value) => UpdateScores();
    partial void OnWarfarinAllergyChanged(bool value) => UpdateScores();
    partial void OnLackOfComplianceChanged(bool value) => UpdateScores();
    partial void OnRecentGIBleedingChanged(bool value) => UpdateScores();
    partial void OnHistoryOfMajorBleedingChanged(bool value) => UpdateScores();
    partial void OnModerateRenalFailureChanged(bool value) => UpdateScores();
    partial void OnModerateHepaticFailureChanged(bool value) => UpdateScores();
    partial void OnModerateThrombocytopeniaChanged(bool value) => UpdateScores();
    partial void OnFrequentFallsChanged(bool value) => UpdateScores();
    partial void OnCognitiveImpairmentChanged(bool value) => UpdateScores();
    partial void OnRecentMajorSurgeryChanged(bool value) => UpdateScores();
    partial void OnOrganicLesionsAtRiskChanged(bool value) => UpdateScores();
    partial void OnAcutePericarditisChanged(bool value) => UpdateScores();
    partial void OnPolypharmacyChanged(bool value) => UpdateScores();
    partial void OnSocialIsolationChanged(bool value) => UpdateScores();
    partial void OnKnownDrugInteractionsChanged(bool value) => UpdateScores();
    partial void OnIrregularDietOrHighVitaminKChanged(bool value) => UpdateScores();
    partial void OnExtremeBMIChanged(bool value) => UpdateScores();
    partial void OnChronicAnemiaChanged(bool value) => UpdateScores();
    partial void OnActiveCancerChanged(bool value) => UpdateScores();
    partial void OnScheduledInvasiveProcedureChanged(bool value) => UpdateScores();
    partial void OnKnownGeneticVariantsChanged(bool value) => UpdateScores();

    /// <summary>
    /// Apre la finestra di dialogo per la valutazione completa
    /// </summary>
    [RelayCommand]
    private void OpenAssessmentDialog()
    {
        var dialog = new PreTaoAssessmentDialog(this)
        {
            Owner = Application.Current.MainWindow
        };

        dialog.ShowDialog();
    }

    /// <summary>
    /// Salva la valutazione
    /// </summary>
    [RelayCommand]
    public async Task SaveAsync()
    {
        try
        {
            IsSaving = true;

            var assessment = new PreTaoAssessment
            {
                PatientId = _patientId,
                AssessmentDate = DateTime.Now,

                // CHA2DS2VASc
                CongestiveHeartFailure = CongestiveHeartFailure,
                Hypertension = Hypertension,
                Age75OrMore = Age75OrMore,
                Diabetes = Diabetes,
                PriorStrokeTiaTE = PriorStrokeTiaTE,
                VascularDisease = VascularDisease,
                Age65To74 = Age65To74,
                Female = Female,

                // HAS-BLED
                HasBledHypertension = HasBledHypertension,
                AbnormalRenalFunction = AbnormalRenalFunction,
                AbnormalLiverFunction = AbnormalLiverFunction,
                StrokeHistory = StrokeHistory,
                BleedingHistory = BleedingHistory,
                LabileINR = LabileINR,
                Elderly = Elderly,
                DrugsPredisposing = DrugsPredisposing,
                AlcoholAbuse = AlcoholAbuse,

                // Controindicazioni assolute
                ActiveMajorBleeding = ActiveMajorBleeding,
                Pregnancy = Pregnancy,
                SevereBloodDyscrasia = SevereBloodDyscrasia,
                RecentNeurosurgery = RecentNeurosurgery,
                IntracranialBleedingOrMalformation = IntracranialBleedingOrMalformation,
                ActivePepticUlcerOrVarices = ActivePepticUlcerOrVarices,
                AcuteBacterialEndocarditis = AcuteBacterialEndocarditis,
                SevereUncontrolledHypertension = SevereUncontrolledHypertension,
                WarfarinAllergy = WarfarinAllergy,
                LackOfCompliance = LackOfCompliance,

                // Controindicazioni relative
                RecentGIBleeding = RecentGIBleeding,
                HistoryOfMajorBleeding = HistoryOfMajorBleeding,
                ModerateRenalFailure = ModerateRenalFailure,
                ModerateHepaticFailure = ModerateHepaticFailure,
                ModerateThrombocytopenia = ModerateThrombocytopenia,
                FrequentFalls = FrequentFalls,
                CognitiveImpairment = CognitiveImpairment,
                RecentMajorSurgery = RecentMajorSurgery,
                OrganicLesionsAtRisk = OrganicLesionsAtRisk,
                AcutePericarditis = AcutePericarditis,

                // Fattori favorenti
                Polypharmacy = Polypharmacy,
                SocialIsolation = SocialIsolation,
                KnownDrugInteractions = KnownDrugInteractions,
                IrregularDietOrHighVitaminK = IrregularDietOrHighVitaminK,
                ExtremeBMI = ExtremeBMI,
                ChronicAnemia = ChronicAnemia,
                ActiveCancer = ActiveCancer,
                ScheduledInvasiveProcedure = ScheduledInvasiveProcedure,
                KnownGeneticVariants = KnownGeneticVariants,

                // Note
                ClinicalNotes = string.IsNullOrWhiteSpace(ClinicalNotes) ? null : ClinicalNotes.Trim(),
                Recommendations = string.IsNullOrWhiteSpace(Recommendations) ? null : Recommendations.Trim(),
                AssessingPhysician = string.IsNullOrWhiteSpace(AssessingPhysician) ? null : AssessingPhysician.Trim(),
                IsApproved = IsApproved,
                ApprovalDate = IsApproved ? DateTime.Now : null
            };

            await _unitOfWork.Database.PreTaoAssessments.AddAsync(assessment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Valutazione pre-TAO salvata per paziente {PatientId}", _patientId);

            // Se non siamo in modalità wizard, mostra il messaggio di conferma
            if (!IsWizardMode)
            {
                _dialogService.ShowInformation(
                    "Valutazione pre-TAO salvata con successo!",
                    "Salvataggio Completato");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il salvataggio della valutazione pre-TAO");
            _dialogService.ShowError(
                $"Errore durante il salvataggio:\n{ex.Message}",
                "Errore");
        }
        finally
        {
            IsSaving = false;
        }
    }
}
