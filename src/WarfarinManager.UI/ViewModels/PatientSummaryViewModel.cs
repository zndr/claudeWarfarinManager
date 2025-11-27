using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.Core.Models;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per la pagina riassuntiva del paziente
/// </summary>
public partial class PatientSummaryViewModel : ObservableObject, INavigationAware
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PatientSummaryViewModel> _logger;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ITTRCalculatorService _ttrCalculator;
    private readonly PatientSummaryPdfService _pdfService;

    [ObservableProperty]
    private int _patientId;

    [ObservableProperty]
    private string _patientFullName = string.Empty;

    [ObservableProperty]
    private string _fiscalCode = string.Empty;

    [ObservableProperty]
    private int _age;

    [ObservableProperty]
    private string _gender = string.Empty;

    // Valutazione Pre-TAO
    [ObservableProperty]
    private int _cha2DS2VAScScore;

    [ObservableProperty]
    private int _hasbledScore;

    [ObservableProperty]
    private string _overallAssessment = "Non disponibile";

    [ObservableProperty]
    private string _recommendations = string.Empty;

    // Indicazione TAO
    [ObservableProperty]
    private string _currentIndication = "Nessuna indicazione attiva";

    [ObservableProperty]
    private DateTime? _taoStartDate;

    [ObservableProperty]
    private string _targetINRRange = "-";

    // TTR
    [ObservableProperty]
    private double _currentTTR;

    [ObservableProperty]
    private string _ttrQualityLevel = string.Empty;

    [ObservableProperty]
    private int _totalControlsCount;

    [ObservableProperty]
    private int _daysInTherapy;

    // Ultimi controlli INR
    [ObservableProperty]
    private ObservableCollection<INRControlSummary> _recentINRControls = new();

    // Bridge Therapy
    [ObservableProperty]
    private ObservableCollection<BridgeTherapySummary> _bridgeTherapies = new();

    [ObservableProperty]
    private bool _hasBridgeTherapies;

    // Campo per memorizzare l'indicazione attiva (serve per il calcolo TTR)
    private Data.Entities.Indication? _activeIndication;

    // Campo per memorizzare il paziente corrente
    private Data.Entities.Patient? _currentPatient;

    // Campo per memorizzare l'assessment corrente
    private Data.Entities.PreTaoAssessment? _currentAssessment;

    public PatientSummaryViewModel(
        IUnitOfWork unitOfWork,
        ILogger<PatientSummaryViewModel> logger,
        INavigationService navigationService,
        IDialogService dialogService,
        ITTRCalculatorService ttrCalculator,
        PatientSummaryPdfService pdfService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _ttrCalculator = ttrCalculator ?? throw new ArgumentNullException(nameof(ttrCalculator));
        _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
    }

    public void OnNavigatedTo(object parameter)
    {
        _logger.LogInformation("OnNavigatedTo chiamato con parametro: {Parameter} (tipo: {Type})", parameter, parameter?.GetType().Name ?? "null");

        if (parameter is int patientId)
        {
            PatientId = patientId;
            _logger.LogInformation("Avvio caricamento riassunto per paziente ID: {PatientId}", PatientId);
            _ = LoadDataAsync();
        }
        else
        {
            _logger.LogWarning("Parametro non valido per OnNavigatedTo: {Parameter}", parameter);
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _logger.LogInformation("Caricamento riassunto paziente {PatientId}", PatientId);

            // Carica dati paziente
            _currentPatient = await _unitOfWork.Patients.GetByIdAsync(PatientId);
            if (_currentPatient == null)
            {
                _logger.LogWarning("Paziente {PatientId} non trovato", PatientId);
                return;
            }

            PatientFullName = $"{_currentPatient.LastName} {_currentPatient.FirstName}";
            FiscalCode = _currentPatient.FiscalCode;
            Age = CalculateAge(_currentPatient.BirthDate);
            Gender = _currentPatient.Gender switch
            {
                Shared.Enums.Gender.Male => "M",
                Shared.Enums.Gender.Female => "F",
                _ => "Altro"
            };

            // Carica valutazione Pre-TAO
            await LoadPreTaoAssessmentAsync();

            // Carica indicazione corrente
            await LoadCurrentIndicationAsync();

            // Carica ultimi controlli INR
            await LoadRecentINRControlsAsync();

            // Carica Bridge Therapy
            await LoadBridgeTherapiesAsync();

            _logger.LogInformation("Riassunto paziente caricato con successo");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il caricamento del riassunto paziente");
        }
    }

    private Task LoadPreTaoAssessmentAsync()
    {
        try
        {
            // Usa Database.Set<PreTaoAssessment>() per accedere all'entità
            var assessments = _unitOfWork.Database.Set<PreTaoAssessment>()
                .Where(p => p.PatientId == PatientId)
                .ToList();

            _logger.LogInformation("Trovate {Count} valutazioni Pre-TAO per paziente {PatientId}", assessments.Count, PatientId);

            _currentAssessment = assessments.OrderByDescending(a => a.AssessmentDate).FirstOrDefault();
            if (_currentAssessment != null)
            {
                Cha2DS2VAScScore = _currentAssessment.CHA2DS2VAScScore;
                HasbledScore = _currentAssessment.HASBLEDScore;
                OverallAssessment = _currentAssessment.OverallAssessment;
                Recommendations = _currentAssessment.Recommendations ?? "Nessuna raccomandazione";

                _logger.LogInformation("Caricata valutazione Pre-TAO: CHA2DS2VASc={Cha2}, HAS-BLED={HasBled}",
                    Cha2DS2VAScScore, HasbledScore);
            }
            else
            {
                _logger.LogWarning("Nessuna valutazione Pre-TAO trovata per paziente {PatientId}", PatientId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore caricamento Pre-TAO assessment");
        }

        return Task.CompletedTask;
    }

    private async Task LoadCurrentIndicationAsync()
    {
        _activeIndication = await _unitOfWork.Indications.GetActiveIndicationAsync(PatientId);

        if (_activeIndication != null)
        {
            CurrentIndication = _activeIndication.IndicationType?.Description ?? "Indicazione non specificata";
            TaoStartDate = _activeIndication.StartDate;
            TargetINRRange = $"{_activeIndication.TargetINRMin:F1} - {_activeIndication.TargetINRMax:F1}";

            _logger.LogInformation("Indicazione attiva caricata: {Indication}, Target INR: {Min}-{Max}",
                CurrentIndication, _activeIndication.TargetINRMin, _activeIndication.TargetINRMax);
        }
        else
        {
            _logger.LogWarning("Nessuna indicazione attiva trovata per paziente {PatientId}", PatientId);
        }
    }

    private async Task LoadRecentINRControlsAsync()
    {
        var controls = await _unitOfWork.INRControls.GetByPatientIdAsync(PatientId);
        var recentControls = controls.OrderByDescending(c => c.ControlDate).Take(10).ToList();

        RecentINRControls.Clear();

        foreach (var control in recentControls)
        {
            // Calcola variazione dosaggio rispetto al precedente
            var previousControl = controls
                .Where(c => c.ControlDate < control.ControlDate)
                .OrderByDescending(c => c.ControlDate)
                .FirstOrDefault();

            double? dosageChange = null;
            string dosageChangeText = "-";

            if (previousControl != null)
            {
                dosageChange = (double)(control.CurrentWeeklyDose - previousControl.CurrentWeeklyDose);
                if (dosageChange > 0)
                    dosageChangeText = $"+{dosageChange:F2} mg";
                else if (dosageChange < 0)
                    dosageChangeText = $"{dosageChange:F2} mg";
                else
                    dosageChangeText = "Invariato";
            }

            RecentINRControls.Add(new INRControlSummary
            {
                ControlDate = control.ControlDate,
                INRValue = (double)control.INRValue,
                WeeklyDose = (double)control.CurrentWeeklyDose,
                DosageChange = dosageChange,
                DosageChangeText = dosageChangeText,
                Notes = control.Notes ?? string.Empty,
                IsInRange = control.IsCompliant
            });
        }

        // Calcola TTR usando il servizio corretto con target INR
        if (recentControls.Any() && _activeIndication != null)
        {
            var allControls = controls.OrderBy(c => c.ControlDate).ToList();

            _logger.LogInformation("Inizio calcolo TTR con ITTRCalculatorService per {Count} controlli", allControls.Count);

            // Converti da Data.Entities.INRControl a Core.Models.INRControl
            var coreControls = allControls.Select(c => new Core.Models.INRControl
            {
                Id = c.Id,
                PatientId = c.PatientId,
                ControlDate = c.ControlDate,
                INRValue = c.INRValue,
                CurrentWeeklyDose = c.CurrentWeeklyDose,
                PhaseOfTherapy = c.PhaseOfTherapy,
                IsCompliant = c.IsCompliant,
                Notes = c.Notes,
                CreatedAt = c.CreatedAt
            }).ToList();

            // Usa il servizio TTR con i target INR corretti
            var ttrResult = _ttrCalculator.CalculateTTR(
                coreControls,
                _activeIndication.TargetINRMin,
                _activeIndication.TargetINRMax);

            CurrentTTR = (double)ttrResult.TTRPercentage;
            TotalControlsCount = allControls.Count;
            DaysInTherapy = ttrResult.TotalDays;

            TtrQualityLevel = CurrentTTR switch
            {
                >= 70 => "Ottimo",
                >= 60 => "Buono",
                >= 50 => "Accettabile",
                _ => "Scarso"
            };

            _logger.LogInformation("TTR calcolato con servizio: {TTR}% (qualità: {Quality}), {Days} giorni totali, {InRange} giorni in range",
                Math.Round(CurrentTTR, 1), TtrQualityLevel, ttrResult.TotalDays, ttrResult.DaysInRange);
        }
        else
        {
            _logger.LogWarning("Impossibile calcolare TTR: controlli={HasControls}, indicazione={HasIndication}",
                recentControls.Any(), _activeIndication != null);
        }
    }

    private async Task LoadBridgeTherapiesAsync()
    {
        var bridges = await _unitOfWork.BridgeTherapyPlans.FindAsync(
            b => b.PatientId == PatientId);

        var sortedBridges = bridges.OrderByDescending(b => b.SurgeryDate).ToList();

        BridgeTherapies.Clear();
        foreach (var bridge in sortedBridges)
        {
            BridgeTherapies.Add(new BridgeTherapySummary
            {
                SurgeryDate = bridge.SurgeryDate,
                SurgeryType = bridge.SurgeryType.ToString(),
                ThromboembolicRisk = bridge.ThromboembolicRisk.ToString(),
                BridgeRecommended = bridge.BridgeRecommended
            });
        }

        HasBridgeTherapies = BridgeTherapies.Any();
    }

    // NOTA: Metodo obsoleto - ora si usa ITTRCalculatorService che calcola correttamente il TTR
    // basandosi sui valori INR effettivi e sul range target (non sul flag IsCompliant)
    /*
    private (double ttr, int daysInTherapy) CalculateTTR(List<INRControl> controls, DateTime startDate)
    {
        if (!controls.Any())
        {
            _logger.LogWarning("Nessun controllo INR disponibile per calcolo TTR");
            return (0, 0);
        }

        var today = DateTime.Today;
        var firstControlDate = controls.First().ControlDate;
        var lastControlDate = controls.Last().ControlDate;

        // Usa il periodo dal primo controllo fino all'ultimo (non fino ad oggi)
        var daysInTherapy = (lastControlDate - firstControlDate).Days;

        _logger.LogInformation("Calcolo TTR: {ControlCount} controlli, dal {First} al {Last}, periodo {Days} giorni",
            controls.Count, firstControlDate.ToShortDateString(), lastControlDate.ToShortDateString(), daysInTherapy);

        if (daysInTherapy <= 0)
        {
            _logger.LogWarning("Periodo di terapia ≤ 0 giorni");
            return (0, 0);
        }

        // Calcola giorni in range terapeutico usando interpolazione lineare
        int daysInRange = 0;

        for (int i = 0; i < controls.Count - 1; i++)
        {
            var current = controls[i];
            var next = controls[i + 1];

            var daysBetween = (next.ControlDate - current.ControlDate).Days;

            if (current.IsCompliant)
            {
                daysInRange += daysBetween;
                _logger.LogDebug("Controllo {Date}: in range, aggiunti {Days} giorni", current.ControlDate.ToShortDateString(), daysBetween);
            }
        }

        var ttr = daysInTherapy > 0 ? (daysInRange * 100.0 / daysInTherapy) : 0;

        _logger.LogInformation("TTR calcolato: {DaysInRange}/{DaysTotal} = {TTR}%", daysInRange, daysInTherapy, Math.Round(ttr, 1));

        return (Math.Round(ttr, 1), daysInTherapy);
    }
    */

    private int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;
        return age;
    }

    /// <summary>
    /// Torna alla lista pazienti
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        try
        {
            _logger.LogInformation("Pulsante Indietro premuto, navigazione verso PatientListViewModel");

            // Se c'è history, torna indietro
            if (_navigationService.CanGoBack)
            {
                _logger.LogInformation("History disponibile, eseguo GoBack()");
                _navigationService.GoBack();
            }
            else
            {
                // Altrimenti naviga esplicitamente alla lista pazienti
                _logger.LogInformation("Nessuna history, navigo esplicitamente a PatientListViewModel");
                _navigationService.NavigateTo<PatientListViewModel>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la navigazione indietro");
            _dialogService.ShowError($"Errore durante la navigazione: {ex.Message}", "Errore");
        }
    }

    /// <summary>
    /// Esporta il riassunto in PDF
    /// </summary>
    [RelayCommand]
    private async Task ExportToPdf()
    {
        try
        {
            _logger.LogInformation("Esportazione riassunto paziente {PatientId} in PDF", PatientId);

            // Verifica che i dati siano stati caricati
            if (_currentPatient == null)
            {
                _dialogService.ShowWarning("Impossibile generare il PDF: dati paziente non caricati.", "Attenzione");
                return;
            }

            // Apri dialogo per salvare il file
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Riassunto_{_currentPatient.LastName}_{_currentPatient.FirstName}_{DateTime.Now:yyyyMMdd}.pdf",
                Title = "Salva Riassunto Paziente"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Genera il PDF usando il servizio
                await _pdfService.GeneratePdfAsync(
                    saveFileDialog.FileName,
                    _currentPatient,
                    _currentAssessment,
                    _activeIndication,
                    RecentINRControls.ToList(),
                    BridgeTherapies.ToList(),
                    CurrentTTR,
                    TtrQualityLevel,
                    TotalControlsCount,
                    DaysInTherapy);

                _logger.LogInformation("PDF generato con successo: {FilePath}", saveFileDialog.FileName);

                // Chiedi se aprire il file
                var result = _dialogService.ShowQuestion(
                    $"PDF generato con successo!\n\nPercorso: {saveFileDialog.FileName}\n\nVuoi aprire il file?",
                    "PDF Generato");

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    // Apri il PDF con il viewer predefinito
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        }
                    };
                    process.Start();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'esportazione PDF");
            _dialogService.ShowError($"Errore durante la generazione del PDF:\n\n{ex.Message}", "Errore");
        }
    }
}

/// <summary>
/// Riassunto controllo INR per tabella
/// </summary>
public class INRControlSummary
{
    public DateTime ControlDate { get; set; }
    public double INRValue { get; set; }
    public double WeeklyDose { get; set; }
    public double? DosageChange { get; set; }
    public string DosageChangeText { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsInRange { get; set; }
}

/// <summary>
/// Riassunto Bridge Therapy per tabella
/// </summary>
public class BridgeTherapySummary
{
    public DateTime SurgeryDate { get; set; }
    public string SurgeryType { get; set; } = string.Empty;
    public string ThromboembolicRisk { get; set; } = string.Empty;
    public bool BridgeRecommended { get; set; }
}
