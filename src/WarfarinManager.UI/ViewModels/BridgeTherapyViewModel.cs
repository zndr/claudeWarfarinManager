using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WarfarinManager.Core.Services;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per la gestione della Bridge Therapy perioperatoria
/// </summary>
public partial class BridgeTherapyViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBridgeTherapyService _bridgeTherapyService;
    
    private Patient? _patient;
    private Indication? _activeIndication;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    private bool _hasSurgeryTypeError;
    
    // === Parametri Intervento ===
    
    [ObservableProperty]
    private DateTime _surgeryDate = DateTime.Today.AddDays(14);
    
    [ObservableProperty]
    private SurgeryType? _selectedSurgeryType;
    
    [ObservableProperty]
    private ObservableCollection<SurgeryTypeItem> _surgeryTypes = new();
    
    // === Valutazione Rischio ===
    
    [ObservableProperty]
    private ThromboembolicRisk _thromboembolicRisk = ThromboembolicRisk.Moderate;
    
    [ObservableProperty]
    private BleedingRisk _bleedingRisk = BleedingRisk.Moderate;
    
    [ObservableProperty]
    private int _cha2ds2vascScore;
    
    [ObservableProperty]
    private bool _hasMechanicalValve;
    
    [ObservableProperty]
    private bool? _hadStrokeTIA;
    
    // === Override rischio manuale ===
    
    [ObservableProperty]
    private bool _useManualRisk;
    
    [ObservableProperty]
    private ThromboembolicRisk _manualTERisk = ThromboembolicRisk.Moderate;
    
    // === Risultato ===
    
    [ObservableProperty]
    private bool _hasResult;
    
    [ObservableProperty]
    private BridgeRecommendation? _fcsaRecommendation;
    
    [ObservableProperty]
    private BridgeRecommendation? _accpRecommendation;
    
    [ObservableProperty]
    private BridgeProtocol? _currentProtocol;
    
    [ObservableProperty]
    private GuidelineType _selectedGuideline = GuidelineType.FCSA;
    
    [ObservableProperty]
    private string _protocolText = string.Empty;
    
    // === Storico ===
    
    [ObservableProperty]
    private ObservableCollection<BridgeTherapyPlanDto> _previousPlans = new();
    
    public BridgeTherapyViewModel(IUnitOfWork unitOfWork, IBridgeTherapyService bridgeTherapyService)
    {
        _unitOfWork = unitOfWork;
        _bridgeTherapyService = bridgeTherapyService;
        
        InitializeSurgeryTypes();
    }
    
    private void InitializeSurgeryTypes()
    {
        // Basso rischio
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.DiagnosticEndoscopy, "Endoscopia diagnostica", "🟢 Basso"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.DiagnosticColonoscopy, "Colonscopia diagnostica", "🟢 Basso"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.CardiacCatheterization, "Cateterismo cardiaco diagnostico", "🟢 Basso"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.Transesophageal, "Eco transesofagea", "🟢 Basso"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.DermatologySingleExcision, "Escissione cutanea minore", "🟢 Basso"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.Ophthalmology, "Cataratta / Laser oculare", "🟢 Basso"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.DentalSingleExtraction, "Estrazione dentaria singola", "🟢 Basso"));
        
        // Rischio moderato
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.EndoscopyWithBiopsy, "Endoscopia con biopsia", "🟡 Moderato"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.Polypectomy, "Polipectomia colonscopica", "🟡 Moderato"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.Cardioversion, "Cardioversione elettrica", "🟡 Moderato"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.PacemakerImplant, "Impianto pacemaker/ICD", "🟡 Moderato"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.ArthroscopyMinor, "Artroscopia minore", "🟡 Moderato"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.LaparoscopicCholecystectomy, "Colecistectomia laparoscopica", "🟡 Moderato"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.TURP, "TURP / Cistoscopia con biopsia", "🟡 Moderato"));
        
        // Alto rischio
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.Neurosurgery, "Neurochirurgia", "🔴 Alto"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.CardiacSurgery, "Cardiochirurgia", "🔴 Alto"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.VascularSurgery, "Chirurgia vascolare maggiore", "🔴 Alto"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.ThoracicSurgery, "Chirurgia toracica", "🔴 Alto"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.AbdominalSurgery, "Chirurgia addominale maggiore", "🔴 Alto"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.HepaticSurgery, "Chirurgia epatica", "🔴 Alto"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.PancreaticSurgery, "Chirurgia pancreatica", "🔴 Alto"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.ProstateSurgery, "Chirurgia prostatica", "🔴 Alto"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.RenalSurgery, "Chirurgia renale", "🔴 Alto"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.MajorOrthopedic, "Chirurgia ortopedica maggiore", "🔴 Alto"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.EpiduralAnesthesia, "Anestesia neuroassiale", "🔴 Alto"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.OphthalmologySurgery, "Chirurgia oculistica maggiore", "🔴 Alto"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.DentalMajor, "Odontoiatria maggiore (≥3 estrazioni)", "🔴 Alto"));
        SurgeryTypes.Add(new SurgeryTypeItem(SurgeryType.Other, "Altra procedura", "⚪ Da valutare"));
    }
    
    public async Task InitializeAsync(int patientId)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        
        try
        {
            // Carica paziente
            _patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
            if (_patient == null)
            {
                ErrorMessage = "Paziente non trovato";
                return;
            }
            
            // Carica indicazione attiva
            var indications = await _unitOfWork.Indications.GetIndicationsByPatientIdAsync(patientId);
            _activeIndication = indications.FirstOrDefault(i => i.IsActive);
            
            // Determina se ha protesi valvolare meccanica
            HasMechanicalValve = _activeIndication?.IndicationType?.Code?.Contains("MECH_VALVE") ?? false;
            
            // Calcola CHA2DS2-VASc se FA
            if (_activeIndication?.IndicationType?.Code?.Contains("FA") == true ||
                _activeIndication?.IndicationType?.Code?.Contains("AFIB") == true)
            {
                Cha2ds2vascScore = _bridgeTherapyService.CalculateCHA2DS2VAScScore(_patient, HadStrokeTIA);
            }
            
            // Carica storico piani bridge
            await LoadPreviousPlansAsync(patientId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Errore durante il caricamento: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task LoadPreviousPlansAsync(int patientId)
    {
        try
        {
            var plans = await _unitOfWork.BridgeTherapyPlans.GetByPatientIdAsync(patientId);
            PreviousPlans.Clear();
            
            foreach (var plan in plans.OrderByDescending(p => p.SurgeryDate))
            {
                PreviousPlans.Add(new BridgeTherapyPlanDto
                {
                    Id = plan.Id,
                    SurgeryDate = plan.SurgeryDate,
                    SurgeryType = plan.SurgeryType,
                    ThromboembolicRisk = plan.ThromboembolicRisk,
                    BridgeRecommended = plan.BridgeRecommended
                });
            }
        }
        catch
        {
            // Ignora errori nel caricamento storico
        }
    }
    
    partial void OnSelectedSurgeryTypeChanged(SurgeryType? value)
    {
        if (value.HasValue)
        {
            BleedingRisk = _bridgeTherapyService.DetermineBleedingRisk(value.Value);
            // Reset errore di validazione quando viene selezionato un tipo
            HasSurgeryTypeError = false;
            ErrorMessage = string.Empty;
        }
        
        // Reset risultato quando cambia il tipo di chirurgia
        HasResult = false;
        CurrentProtocol = null;
    }
    
    partial void OnUseManualRiskChanged(bool value)
    {
        if (!value)
        {
            // Ricalcola rischio automatico
            RecalculateAutomaticRisk();
        }
        
        HasResult = false;
        CurrentProtocol = null;
    }
    
    partial void OnHadStrokeTIAChanged(bool? value)
    {
        if (_patient != null)
        {
            Cha2ds2vascScore = _bridgeTherapyService.CalculateCHA2DS2VAScScore(_patient, value);
            RecalculateAutomaticRisk();
        }
    }
    
    private void RecalculateAutomaticRisk()
    {
        if (_patient != null && !UseManualRisk)
        {
            ThromboembolicRisk = _bridgeTherapyService.DetermineThromboembolicRisk(
                _patient, _activeIndication, Cha2ds2vascScore);
        }
    }
    
    [RelayCommand]
    private void CalculateBridge()
    {
        // Reset errori
        HasSurgeryTypeError = false;
        ErrorMessage = string.Empty;
        
        if (_patient == null)
        {
            ErrorMessage = "Paziente non caricato";
            return;
        }
        
        if (!SelectedSurgeryType.HasValue)
        {
            HasSurgeryTypeError = true;
            ErrorMessage = "⚠ Selezionare il tipo di chirurgia per calcolare il protocollo";
            // Il focus verrà gestito dalla View tramite binding
            return;
        }
        
        if (SurgeryDate <= DateTime.Today)
        {
            ErrorMessage = "La data dell'intervento deve essere nel futuro";
            return;
        }
        
        try
        {
            var effectiveTERisk = UseManualRisk ? ManualTERisk : ThromboembolicRisk;
            
            // Ottieni raccomandazioni FCSA e ACCP
            FcsaRecommendation = _bridgeTherapyService.GetFCSARecommendation(
                effectiveTERisk, BleedingRisk, HasMechanicalValve);
            
            AccpRecommendation = _bridgeTherapyService.GetACCPRecommendation(
                effectiveTERisk, BleedingRisk, HasMechanicalValve);
            
            // Genera protocollo con le linee guida selezionate
            var selectedRec = SelectedGuideline == GuidelineType.FCSA ? FcsaRecommendation : AccpRecommendation;
            
            CurrentProtocol = _bridgeTherapyService.GenerateProtocol(
                _patient,
                SurgeryDate,
                SelectedSurgeryType.Value,
                effectiveTERisk,
                BleedingRisk,
                SelectedGuideline,
                selectedRec!.BridgeRecommended,
                selectedRec.DosageType);
            
            // Genera testo protocollo
            ProtocolText = _bridgeTherapyService.FormatProtocolForExport(CurrentProtocol, _patient);
            
            HasResult = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Errore nel calcolo: {ex.Message}";
            HasResult = false;
        }
    }
    
    [RelayCommand]
    private async Task SavePlanAsync()
    {
        if (_patient == null || CurrentProtocol == null)
            return;
        
        IsLoading = true;
        
        try
        {
            var plan = new BridgeTherapyPlan
            {
                PatientId = _patient.Id,
                SurgeryDate = CurrentProtocol.SurgeryDate,
                SurgeryType = CurrentProtocol.SurgeryType,
                ThromboembolicRisk = CurrentProtocol.TERisk,
                BridgeRecommended = CurrentProtocol.BridgeRecommended,
                WarfarinStopDate = CurrentProtocol.WarfarinStopDate,
                EBPMStartDate = CurrentProtocol.EBPMStartDate,
                EBPMLastDoseDate = CurrentProtocol.EBPMLastDoseDate,
                WarfarinResumeDate = CurrentProtocol.WarfarinResumeDate,
                EBPMResumeDate = CurrentProtocol.EBPMResumeDate,
                ProtocolText = ProtocolText,
                Notes = string.Join("; ", CurrentProtocol.ClinicalNotes)
            };
            
            await _unitOfWork.BridgeTherapyPlans.AddAsync(plan);
            await _unitOfWork.SaveChangesAsync();
            
            // Ricarica storico
            await LoadPreviousPlansAsync(_patient.Id);
            
            MessageBox.Show("Piano bridge therapy salvato con successo!", 
                "Salvataggio completato", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Errore durante il salvataggio: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void CopyToClipboard()
    {
        if (!string.IsNullOrEmpty(ProtocolText))
        {
            Clipboard.Clear();
            Clipboard.SetText(ProtocolText);
            MessageBox.Show("Protocollo copiato negli appunti!", 
                "Copia completata", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
    }
    
    [RelayCommand]
    private async Task ExportToFileAsync()
    {
        if (_patient == null || string.IsNullOrEmpty(ProtocolText))
            return;
        
        var saveDialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = $"BridgeTherapy_{_patient.LastName}_{_patient.FirstName}_{SurgeryDate:yyyyMMdd}.txt",
            Filter = "File di testo|*.txt",
            Title = "Esporta protocollo bridge therapy"
        };
        
        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                await System.IO.File.WriteAllTextAsync(saveDialog.FileName, ProtocolText);
                MessageBox.Show($"Protocollo esportato in:\n{saveDialog.FileName}", 
                    "Esportazione completata", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Errore durante l'esportazione: {ex.Message}";
            }
        }
    }
}

/// <summary>
/// Item per la ComboBox dei tipi di chirurgia
/// </summary>
public class SurgeryTypeItem
{
    public SurgeryType Type { get; }
    public string Description { get; }
    public string RiskLevel { get; }
    public string DisplayText => $"{RiskLevel} {Description}";
    
    public SurgeryTypeItem(SurgeryType type, string description, string riskLevel)
    {
        Type = type;
        Description = description;
        RiskLevel = riskLevel;
    }
}

/// <summary>
/// DTO per la visualizzazione dei piani bridge precedenti
/// </summary>
public class BridgeTherapyPlanDto
{
    public int Id { get; set; }
    public DateTime SurgeryDate { get; set; }
    public SurgeryType SurgeryType { get; set; }
    public ThromboembolicRisk ThromboembolicRisk { get; set; }
    public bool BridgeRecommended { get; set; }
    
    public string BridgeStatus => BridgeRecommended ? "✓ Bridge raccomandato" : "✗ No bridge";
    public string SurgeryDateFormatted => SurgeryDate.ToString("dd/MM/yyyy");
    public string SurgeryTypeDescription => SurgeryType.ToString();
    public string RiskDescription => ThromboembolicRisk.ToString();
}
