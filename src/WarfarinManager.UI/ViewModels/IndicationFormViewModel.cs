using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.UI.Models;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per il form di inserimento nuova indicazione terapeutica
/// </summary>
public partial class IndicationFormViewModel : ObservableObject, INavigationAware
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<IndicationFormViewModel> _logger;

    [ObservableProperty]
    private int _patientId;

    [ObservableProperty]
    private string _patientName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<IndicationTypeDto> _availableIndications = new();

    [ObservableProperty]
    private IndicationTypeDto? _selectedIndicationType;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string? _indicationError;

    [ObservableProperty]
    private string? _startDateError;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private bool _isLoading;

    // Computed properties per visualizzazione
    [ObservableProperty]
    private string _targetINRDisplay = "-";

    [ObservableProperty]
    private string _typicalDurationDisplay = "-";

    public IndicationFormViewModel(
        IUnitOfWork unitOfWork,
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<IndicationFormViewModel> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Chiamato quando si naviga verso questa view
    /// </summary>
    public void OnNavigatedTo(object parameter)
    {
        if (parameter is int patientId)
        {
            PatientId = patientId;
            _ = LoadDataAsync();
        }
    }

    /// <summary>
    /// Carica i dati necessari
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            _logger.LogInformation("Caricamento dati form indicazione per paziente {PatientId}", PatientId);

            // Carica nome paziente
            var patient = await _unitOfWork.Patients.GetByIdAsync(PatientId);
            if (patient == null)
            {
                _dialogService.ShowError("Paziente non trovato", "Errore");
                Cancel();
                return;
            }

            PatientName = $"{patient.LastName} {patient.FirstName}";

            // Carica tutti i tipi di indicazione disponibili
            var indicationTypes = await _unitOfWork.Database.IndicationTypes
                .OrderBy(it => it.Category)
                .ThenBy(it => it.Description)
                .ToListAsync();

            var dtos = indicationTypes
                .Select(MapIndicationTypeToDto)
                .ToList();

            AvailableIndications = new ObservableCollection<IndicationTypeDto>(dtos);

            _logger.LogInformation("Caricate {Count} indicazioni disponibili", AvailableIndications.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il caricamento dati form indicazione");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Aggiorna i campi dipendenti quando cambia l'indicazione selezionata
    /// </summary>
    partial void OnSelectedIndicationTypeChanged(IndicationTypeDto? value)
    {
        if (value != null)
        {
            TargetINRDisplay = value.TargetINRRange;
            TypicalDurationDisplay = value.TypicalDuration ?? "-";
            
            // Pulisci l'errore quando l'utente seleziona un'indicazione
            IndicationError = null;
        }
        else
        {
            TargetINRDisplay = "-";
            TypicalDurationDisplay = "-";
        }
    }

    /// <summary>
    /// Salva la nuova indicazione
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            // Validazione
            if (!ValidateForm())
                return;

            IsSaving = true;
            _logger.LogInformation("Salvataggio nuova indicazione per paziente {PatientId}", PatientId);

            // Verifica se esiste già un'indicazione attiva
            var activeIndication = await _unitOfWork.Database.Indications
                .FirstOrDefaultAsync(i => i.PatientId == PatientId && i.IsActive);

            if (activeIndication != null)
            {
                // Chiedi conferma per terminare l'indicazione attiva
                var result = _dialogService.ShowQuestion(
                    $"Esiste già un'indicazione attiva:\n\n" +
                    $"{activeIndication.IndicationType?.Description}\n" +
                    $"Dal: {activeIndication.StartDate:dd/MM/yyyy}\n\n" +
                    $"Vuoi terminarla e attivare la nuova indicazione?",
                    "Indicazione Attiva Presente");

                if (result != System.Windows.MessageBoxResult.Yes)
                {
                    IsSaving = false;
                    return;
                }

                // Termina l'indicazione attiva
                activeIndication.EndDate = StartDate.AddDays(-1);
                activeIndication.IsActive = false;
                activeIndication.ChangeReason = "Cambio indicazione terapeutica";
            }

            // Crea la nuova indicazione
            var indication = new Indication
            {
                PatientId = PatientId,
                IndicationTypeId = SelectedIndicationType!.Id,
                TargetINRMin = SelectedIndicationType.TargetINRMin,
                TargetINRMax = SelectedIndicationType.TargetINRMax,
                StartDate = StartDate,
                EndDate = null,
                IsActive = true,
                ChangeReason = activeIndication != null ? "Sostituzione indicazione precedente" : null,
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim()
            };

            await _unitOfWork.Database.Indications.AddAsync(indication);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Indicazione salvata con successo: ID {IndicationId}", indication.Id);

            _dialogService.ShowInformation(
                $"Indicazione aggiunta con successo!\n\n" +
                $"Indicazione: {SelectedIndicationType.Description}\n" +
                $"Target INR: {SelectedIndicationType.TargetINRRange}",
                "Successo");

            // Torna ai dettagli del paziente
            _navigationService.NavigateTo<PatientDetailsViewModel>(PatientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il salvataggio dell'indicazione");
            _dialogService.ShowError($"Errore durante il salvataggio: {ex.Message}", "Errore");
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// Annulla e torna indietro
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        if (HasUnsavedChanges())
        {
            var result = _dialogService.ShowQuestion(
                "Ci sono modifiche non salvate. Vuoi uscire senza salvare?",
                "Conferma Annullamento");

            if (result != System.Windows.MessageBoxResult.Yes)
                return;
        }

        _navigationService.NavigateTo<PatientDetailsViewModel>(PatientId);
    }

    /// <summary>
    /// Valida il form
    /// </summary>
    private bool ValidateForm()
    {
        bool isValid = true;

        // Validazione indicazione
        if (SelectedIndicationType == null)
        {
            IndicationError = "Seleziona un'indicazione terapeutica";
            isValid = false;
        }
        else
        {
            IndicationError = null;
        }

        // Validazione data inizio
        if (StartDate > DateTime.Today)
        {
            StartDateError = "La data di inizio non può essere futura";
            isValid = false;
        }
        else if (StartDate < DateTime.Today.AddYears(-10))
        {
            StartDateError = "La data di inizio non può essere più vecchia di 10 anni";
            isValid = false;
        }
        else
        {
            StartDateError = null;
        }

        return isValid;
    }

    /// <summary>
    /// Verifica se ci sono modifiche non salvate
    /// </summary>
    private bool HasUnsavedChanges()
    {
        return SelectedIndicationType != null ||
               StartDate != DateTime.Today ||
               !string.IsNullOrWhiteSpace(Notes);
    }

    /// <summary>
    /// Mappa IndicationType entity a DTO
    /// </summary>
    private IndicationTypeDto MapIndicationTypeToDto(IndicationType indicationType)
    {
        return new IndicationTypeDto
        {
            Id = indicationType.Id,
            Code = indicationType.Code,
            Category = indicationType.Category,
            Description = indicationType.Description,
            TargetINRMin = indicationType.TargetINRMin,
            TargetINRMax = indicationType.TargetINRMax,
            TypicalDuration = indicationType.TypicalDuration
        };
    }
}
