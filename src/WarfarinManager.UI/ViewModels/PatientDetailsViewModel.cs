using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.UI.Models;
using WarfarinManager.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using WarfarinManager.UI.Views.INR;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per la vista dettagli paziente con tab multiple
/// </summary>
public partial class PatientDetailsViewModel : ObservableObject, INavigationAware
    {
    private readonly IUnitOfWork _unitOfWork;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<PatientDetailsViewModel> _logger;
    private readonly IServiceProvider _serviceProvider;  // ← AGGIUNTO
    [ObservableProperty]
    private int _patientId;

    [ObservableProperty]
    private PatientDto? _patient;

    [ObservableProperty]
    private ObservableCollection<IndicationDto> _indications = new();

    [ObservableProperty]
    private IndicationDto? _selectedIndication;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    public PatientDetailsViewModel(
        IUnitOfWork unitOfWork,
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<PatientDetailsViewModel> logger,
        IServiceProvider serviceProvider)  // ← AGGIUNTO PARAMETRO
        {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));  // ← AGGIUNTO
        }
    /// <summary>
    /// Chiamato quando si naviga verso questa view
    /// </summary>
    public void OnNavigatedTo(object parameter)
    {
        if (parameter is int patientId)
        {
            PatientId = patientId;
            _ = LoadPatientDataAsync(PatientId);
        }
    }

    /// <summary>
    /// Carica i dati del paziente
    /// </summary>
    [RelayCommand]
    public async Task LoadPatientDataAsync(int patientId)  // ← public invece di private
    {
            try
        {
            IsLoading = true;
            _logger.LogInformation("Caricamento dettagli paziente ID: {PatientId}", PatientId);

            var patient = await _unitOfWork.Patients.GetByIdAsync(PatientId);
            if (patient == null)
            {
                _dialogService.ShowError("Paziente non trovato", "Errore");
                _navigationService.GoBack();
                return;
            }

            Patient = MapPatientToDto(patient);

            // Carica le indicazioni
            await LoadIndicationsAsync();

            _logger.LogInformation("Paziente caricato: {FullName}", Patient.FullName);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Carica le indicazioni del paziente
    /// </summary>
    private async Task LoadIndicationsAsync()
    {
        try
        {
            var indications = await _unitOfWork.Database.Indications
                .Where(i => i.PatientId == PatientId)
                .Include(i => i.IndicationType)
                .OrderByDescending(i => i.IsActive)
                .ThenByDescending(i => i.StartDate)
                .ToListAsync();

            var indicationDtos = indications
                .Select(MapIndicationToDto)
                .ToList();

            Indications = new ObservableCollection<IndicationDto>(indicationDtos);

            _logger.LogInformation("Caricate {Count} indicazioni per paziente {PatientId}", 
                Indications.Count, PatientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il caricamento delle indicazioni");
            throw;
        }
    }

    /// <summary>
    /// Torna alla lista pazienti
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        _navigationService.NavigateTo<PatientListViewModel>();
    }

    /// <summary>
    /// Apre il form per modificare l'anagrafica
    /// </summary>
    [RelayCommand]
    private void EditPatient()
    {
        _dialogService.ShowInformation(
            "Modifica anagrafica paziente\n\n(Funzionalità in sviluppo)",
            "Info");
    }

    /// <summary>
    /// Apre il form per aggiungere una nuova indicazione
    /// </summary>
    [RelayCommand]
    private void AddIndication()
    {
        try
        {
            _logger.LogInformation("Apertura form nuova indicazione per paziente {PatientId}", PatientId);
            
            // Naviga al form indicazione passando l'ID del paziente
            _navigationService.NavigateTo<IndicationFormViewModel>(PatientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore apertura form indicazione");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
    }

    /// <summary>
    /// Apre la vista per gestire i controlli INR del paziente
    /// </summary>
    [RelayCommand]
    private async Task OpenINRControlAsync()
        {
        try
            {
            if (Patient == null) return;

            _logger.LogInformation("Apertura gestione INR per paziente {PatientId}", PatientId);

            // Risolvi INRControlView e ViewModel dal DI
            var inrView = _serviceProvider.GetRequiredService<INRControlView>();
            var inrViewModel = _serviceProvider.GetRequiredService<INRControlViewModel>();

            // Carica i dati del paziente nel ViewModel
            await inrViewModel.LoadPatientDataAsync(PatientId);

            // Assegna il DataContext
            inrView.DataContext = inrViewModel;

            // Mostra la finestra
            inrView.ShowDialog();

            // Dopo la chiusura, ricarica i dati del paziente (potrebbero essere cambiati)
            await LoadPatientDataAsync(PatientId);
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Errore apertura gestione INR");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
            }
        }

    /// <summary>
    /// Termina l'indicazione selezionata
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanEndIndication))]
    private async Task EndIndicationAsync()
    {
        if (SelectedIndication == null)
            return;

        try
        {
            var confirm = _dialogService.ShowQuestion(
                $"Vuoi terminare l'indicazione '{SelectedIndication.Description}'?\n\n" +
                $"Data inizio: {SelectedIndication.StartDate:dd/MM/yyyy}\n" +
                $"Questa azione non può essere annullata.",
                "Conferma Termine Indicazione");

            if (confirm != System.Windows.MessageBoxResult.Yes)
                return;

            var indication = await _unitOfWork.Database.Indications
                .FirstOrDefaultAsync(i => i.Id == SelectedIndication.Id);

            if (indication == null)
            {
                _dialogService.ShowError("Indicazione non trovata", "Errore");
                return;
            }

            indication.EndDate = DateTime.Today;
            indication.IsActive = false;

            await _unitOfWork.SaveChangesAsync();

            _dialogService.ShowInformation("Indicazione terminata con successo", "Successo");
            await LoadIndicationsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il termine dell'indicazione");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
    }

    private bool CanEndIndication() => SelectedIndication != null && SelectedIndication.IsActive;

    partial void OnSelectedIndicationChanged(IndicationDto? value)
    {
        EndIndicationCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Mappa Patient entity a DTO
    /// </summary>
    private PatientDto MapPatientToDto(Data.Entities.Patient patient)
    {
        return new PatientDto
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            BirthDate = patient.BirthDate,
            FiscalCode = patient.FiscalCode,
            Gender = patient.Gender?.ToString(),
            Phone = patient.Phone,
            Email = patient.Email,
            IsSlowMetabolizer = patient.IsSlowMetabolizer,
            // TODO: Caricare indicazione attiva, ultimo INR, TTR
            ActiveIndication = null,
            LastINR = null,
            LastINRDate = null,
            TTRPercentage = null,
            NextControlDate = null
        };
    }

    /// <summary>
    /// Mappa Indication entity a DTO
    /// </summary>
    private IndicationDto MapIndicationToDto(Data.Entities.Indication indication)
    {
        return new IndicationDto
        {
            Id = indication.Id,
            PatientId = indication.PatientId,
            IndicationTypeCode = indication.IndicationType?.Code ?? string.Empty,
            Description = indication.IndicationType?.Description ?? "N/D",
            Category = indication.IndicationType?.Category ?? string.Empty,
            TargetINRMin = indication.TargetINRMin,
            TargetINRMax = indication.TargetINRMax,
            StartDate = indication.StartDate,
            EndDate = indication.EndDate,
            IsActive = indication.IsActive,
            ChangeReason = indication.ChangeReason,
            Notes = indication.Notes,
            TypicalDuration = indication.IndicationType?.TypicalDuration
        };
    }
}
