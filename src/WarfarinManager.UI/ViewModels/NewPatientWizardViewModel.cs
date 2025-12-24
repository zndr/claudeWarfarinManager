using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per il wizard obbligatorio di configurazione nuovo paziente naive
/// Step 1: Indicazione alla TAO
/// Step 2: Valutazione Pre-TAO (controindicazioni e valutatore)
/// Step 3: Score CHA2DS2-VASc
/// Step 4: Score HAS-BLED
/// </summary>
public partial class NewPatientWizardViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<NewPatientWizardViewModel> _logger;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private int _patientId;

    [ObservableProperty]
    private string _patientName = string.Empty;

    [ObservableProperty]
    private int _currentStep = 1;

    [ObservableProperty]
    private string _currentStepTitle = "Indicazione alla TAO";

    [ObservableProperty]
    private bool _canGoNext;

    [ObservableProperty]
    private bool _canGoPrevious;

    [ObservableProperty]
    private bool _isLastStep;

    [ObservableProperty]
    private bool _shouldCloseWizard;

    [ObservableProperty]
    private bool _shouldOpenINRForm;

    // ViewModels per ogni step
    [ObservableProperty]
    private IndicationFormViewModel? _indicationFormViewModel;

    [ObservableProperty]
    private PreTaoAssessmentViewModel? _preTaoAssessmentViewModel;

    // Flag per tracciare il completamento di ogni step
    private bool _indicationCompleted;
    private bool _preTaoAssessmentCompleted;
    private bool _cha2ds2VascCompleted;
    private bool _hasBledCompleted;

    public NewPatientWizardViewModel(
        IUnitOfWork unitOfWork,
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<NewPatientWizardViewModel> logger,
        IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        UpdateStepState();
    }

    /// <summary>
    /// Inizializza il wizard con i dati del paziente
    /// </summary>
    public async Task InitializeAsync(int patientId)
    {
        try
        {
            PatientId = patientId;

            var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
            if (patient == null)
            {
                _dialogService.ShowError("Paziente non trovato", "Errore");
                return;
            }

            PatientName = patient.FullName;

            // Inizializza i ViewModel per ogni step
            IndicationFormViewModel = _serviceProvider.GetRequiredService<IndicationFormViewModel>();
            PreTaoAssessmentViewModel = _serviceProvider.GetRequiredService<PreTaoAssessmentViewModel>();

            // IMPORTANTE: Imposta modalità wizard per evitare navigazione automatica
            IndicationFormViewModel.IsWizardMode = true;
            PreTaoAssessmentViewModel.IsWizardMode = true;

            // Carica i dati per lo step corrente
            await LoadCurrentStepAsync();

            _logger.LogInformation("Wizard inizializzato per paziente {PatientId} - {PatientName}", patientId, PatientName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'inizializzazione del wizard");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
    }

    /// <summary>
    /// Carica i dati per lo step corrente
    /// </summary>
    private async Task LoadCurrentStepAsync()
    {
        try
        {
            switch (CurrentStep)
            {
                case 1: // Indicazione alla TAO
                    CurrentStepTitle = "Step 1/4: Indicazione alla TAO";
                    if (IndicationFormViewModel != null)
                    {
                        IndicationFormViewModel.OnNavigatedTo(PatientId);
                    }
                    break;

                case 2: // Valutazione Pre-TAO (controindicazioni e valutatore)
                    CurrentStepTitle = "Step 2/4: Valutazione Pre-TAO";
                    if (PreTaoAssessmentViewModel != null)
                    {
                        await PreTaoAssessmentViewModel.InitializeAsync(PatientId);
                    }
                    break;

                case 3: // Score CHA2DS2-VASc
                    CurrentStepTitle = "Step 3/4: Score CHA₂DS₂-VASc";
                    if (PreTaoAssessmentViewModel != null)
                    {
                        await PreTaoAssessmentViewModel.InitializeAsync(PatientId);
                    }
                    break;

                case 4: // Score HAS-BLED
                    CurrentStepTitle = "Step 4/4: Score HAS-BLED";
                    if (PreTaoAssessmentViewModel != null)
                    {
                        await PreTaoAssessmentViewModel.InitializeAsync(PatientId);
                    }
                    break;
            }

            UpdateStepState();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il caricamento dello step {Step}", CurrentStep);
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
    }

    /// <summary>
    /// Passa allo step successivo
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextStepAsync()
    {
        try
        {
            // Salva lo step corrente prima di passare al prossimo
            if (!await SaveCurrentStepAsync())
            {
                return;
            }

            CurrentStep++;
            await LoadCurrentStepAsync();

            _logger.LogInformation("Passato allo step {Step}", CurrentStep);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il passaggio allo step successivo");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
    }

    /// <summary>
    /// Torna allo step precedente
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private async Task PreviousStepAsync()
    {
        try
        {
            CurrentStep--;
            await LoadCurrentStepAsync();

            _logger.LogInformation("Tornato allo step {Step}", CurrentStep);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il ritorno allo step precedente");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
    }

    /// <summary>
    /// Salva lo step corrente
    /// </summary>
    private async Task<bool> SaveCurrentStepAsync()
    {
        try
        {
            switch (CurrentStep)
            {
                case 1: // Indicazione alla TAO
                    if (IndicationFormViewModel?.SelectedIndicationType == null)
                    {
                        _dialogService.ShowWarning(
                            "Devi selezionare un'indicazione alla TAO prima di procedere.",
                            "Indicazione Obbligatoria");
                        return false;
                    }

                    // Salva l'indicazione
                    await IndicationFormViewModel.SaveCommand.ExecuteAsync(null);
                    _indicationCompleted = true;
                    _logger.LogInformation("Indicazione salvata per paziente {PatientId}", PatientId);
                    return true;

                case 2: // Valutazione Pre-TAO (controindicazioni e valutatore)
                    // Verifica presenza di controindicazioni assolute
                    if (PreTaoAssessmentViewModel != null && PreTaoAssessmentViewModel.HasAbsoluteContraindications)
                    {
                        var result = _dialogService.ShowQuestion(
                            "⚠️ ATTENZIONE - Controindicazioni Assolute Rilevate\n\n" +
                            "Sono state contrassegnate una o più controindicazioni ASSOLUTE alla terapia anticoagulante orale.\n\n" +
                            "In presenza di controindicazioni assolute, la TAO è generalmente CONTROINDICATA.\n\n" +
                            "Desideri comunque continuare con la configurazione del paziente?",
                            "Controindicazioni Assolute");

                        if (result != System.Windows.MessageBoxResult.Yes)
                        {
                            return false;
                        }
                    }

                    _preTaoAssessmentCompleted = true;
                    return true;

                case 3: // Score CHA2DS2-VASc
                    // Non richiesto salvataggio intermedio, passa allo step successivo
                    _cha2ds2VascCompleted = true;
                    return true;

                case 4: // Score HAS-BLED
                    if (PreTaoAssessmentViewModel == null)
                    {
                        _dialogService.ShowWarning(
                            "Devi completare la valutazione prima di procedere.",
                            "Valutazione Obbligatoria");
                        return false;
                    }

                    // Verifica che la valutazione sia stata approvata
                    if (!PreTaoAssessmentViewModel.IsApproved)
                    {
                        var result = _dialogService.ShowQuestion(
                            "La valutazione pre-TAO non è stata approvata.\n\n" +
                            "Per procedere è necessario approvare la valutazione.\n\n" +
                            "Vuoi approvarla ora?",
                            "Approvazione Richiesta");

                        if (result == System.Windows.MessageBoxResult.Yes)
                        {
                            PreTaoAssessmentViewModel.IsApproved = true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    // Salva la valutazione pre-TAO completa
                    await PreTaoAssessmentViewModel.SaveCommand.ExecuteAsync(null);
                    _hasBledCompleted = true;
                    _logger.LogInformation("Valutazione Pre-TAO completa salvata per paziente {PatientId}", PatientId);
                    return true;

                default:
                    return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il salvataggio dello step {Step}", CurrentStep);
            _dialogService.ShowError($"Errore durante il salvataggio: {ex.Message}", "Errore");
            return false;
        }
    }

    /// <summary>
    /// Completa il wizard e marca il paziente come configurato
    /// </summary>
    [RelayCommand]
    private async Task CompleteWizardAsync()
    {
        try
        {
            // Salva l'ultimo step
            if (!await SaveCurrentStepAsync())
            {
                return;
            }

            // Marca il paziente come wizard completato
            var patient = await _unitOfWork.Patients.GetByIdAsync(PatientId);
            if (patient != null)
            {
                patient.IsInitialWizardCompleted = true;
                await _unitOfWork.Patients.UpdateAsync(patient);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Wizard completato per paziente {PatientId}", PatientId);

                // Chiedi se vuole inserire il primo valore INR
                var result = _dialogService.ShowQuestion(
                    "✅ Configurazione iniziale completata con successo!\n\n" +
                    "Vuoi inserire subito il primo valore INR per questo paziente?",
                    "Wizard Completato");

                ShouldOpenINRForm = (result == System.Windows.MessageBoxResult.Yes);
                ShouldCloseWizard = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il completamento del wizard");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
    }

    /// <summary>
    /// Annulla il wizard
    /// </summary>
    [RelayCommand]
    private void CancelWizard()
    {
        var result = _dialogService.ShowQuestion(
            "Sei sicuro di voler annullare la configurazione iniziale?\n\n" +
            "ATTENZIONE: Non potrai inserire controlli INR finché non completerai questa configurazione.",
            "Conferma Annullamento");

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            _logger.LogInformation("Wizard annullato per paziente {PatientId}", PatientId);
            _navigationService.NavigateTo<PatientDetailsViewModel>(PatientId);
        }
    }

    /// <summary>
    /// Aggiorna lo stato dei pulsanti di navigazione
    /// </summary>
    private void UpdateStepState()
    {
        CanGoPrevious = CurrentStep > 1;
        CanGoNext = CurrentStep < 4;
        IsLastStep = CurrentStep == 4;

        NextStepCommand.NotifyCanExecuteChanged();
        PreviousStepCommand.NotifyCanExecuteChanged();
    }

    partial void OnCurrentStepChanged(int value)
    {
        UpdateStepState();
    }
}
