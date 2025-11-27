using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.UI.Models;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per la lista pazienti con ricerca e filtri
/// </summary>
public partial class PatientListViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITTRCalculatorService _ttrCalculator;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<PatientListViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<PatientDto> _patients = new();

    [ObservableProperty]
    private ObservableCollection<PatientDto> _filteredPatients = new();

    [ObservableProperty]
    private PatientDto? _selectedPatient;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalPatientsCount;

    [ObservableProperty]
    private int _filteredPatientsCount;

    public PatientListViewModel(
        IUnitOfWork unitOfWork,
        ITTRCalculatorService ttrCalculator,
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<PatientListViewModel> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _ttrCalculator = ttrCalculator ?? throw new ArgumentNullException(nameof(ttrCalculator));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Carica tutti i pazienti dal database con dati INR e TTR
    /// </summary>
    [RelayCommand]
    private async Task LoadPatientsAsync()
    {
        try
        {
            IsLoading = true;
            _logger.LogInformation("Caricamento lista pazienti...");

            // Carica pazienti con indicazioni e controlli INR
            var patients = await _unitOfWork.Database.Patients
                .Include(p => p.Indications)
                    .ThenInclude(i => i.IndicationType)
                .Include(p => p.INRControls)
                    .ThenInclude(c => c.DosageSuggestions)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();

            var patientDtos = new List<PatientDto>();

            foreach (var patient in patients)
            {
                var dto = await MapToDtoWithStatsAsync(patient);
                patientDtos.Add(dto);
            }

            Patients = new ObservableCollection<PatientDto>(patientDtos);
            FilteredPatients = new ObservableCollection<PatientDto>(patientDtos);
            
            TotalPatientsCount = Patients.Count;
            FilteredPatientsCount = FilteredPatients.Count;

            _logger.LogInformation("Caricati {Count} pazienti", TotalPatientsCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il caricamento dei pazienti");
            _dialogService.ShowError(
                $"Errore durante il caricamento dei pazienti:\n{ex.Message}",
                "Errore");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Esegue la ricerca sui pazienti
    /// </summary>
    [RelayCommand]
    private void Search()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Nessuna ricerca: mostra tutti
                FilteredPatients = new ObservableCollection<PatientDto>(Patients);
            }
            else
            {
                var searchLower = SearchText.ToLowerInvariant();
                
                var filtered = Patients
                    .Where(p =>
                        p.FirstName.ToLowerInvariant().Contains(searchLower) ||
                        p.LastName.ToLowerInvariant().Contains(searchLower) ||
                        p.FiscalCode.ToLowerInvariant().Contains(searchLower) ||
                        p.FullName.ToLowerInvariant().Contains(searchLower))
                    .ToList();

                FilteredPatients = new ObservableCollection<PatientDto>(filtered);
            }

            FilteredPatientsCount = FilteredPatients.Count;
            _logger.LogDebug("Ricerca '{SearchText}': trovati {Count} risultati", SearchText, FilteredPatientsCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la ricerca pazienti");
        }
    }

    /// <summary>
    /// Aggiorna automaticamente la ricerca quando cambia il testo
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        Search();
    }

    /// <summary>
    /// Apre i dettagli del paziente selezionato
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOpenPatientDetails))]
    private void OpenPatientDetails()
    {
        if (SelectedPatient == null)
            return;

        try
        {
            _logger.LogInformation("Apertura dettagli paziente ID: {PatientId}", SelectedPatient.Id);
            
            // Naviga ai dettagli passando l'ID del paziente
            _navigationService.NavigateTo<PatientDetailsViewModel>(SelectedPatient.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore apertura dettagli paziente");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
    }

    private bool CanOpenPatientDetails() => SelectedPatient != null;

    /// <summary>
    /// Apre il riassunto clinico del paziente selezionato
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOpenPatientSummary))]
    private void OpenPatientSummary()
    {
        if (SelectedPatient == null)
            return;

        try
        {
            _logger.LogInformation("Apertura riassunto paziente ID: {PatientId}", SelectedPatient.Id);

            // Naviga al riassunto passando l'ID del paziente
            _navigationService.NavigateTo<PatientSummaryViewModel>(SelectedPatient.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore apertura riassunto paziente");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
    }

    private bool CanOpenPatientSummary() => SelectedPatient != null;

    /// <summary>
    /// Aggiorna il CanExecute quando cambia la selezione
    /// </summary>
    partial void OnSelectedPatientChanged(PatientDto? value)
    {
        OpenPatientDetailsCommand.NotifyCanExecuteChanged();
        OpenPatientSummaryCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Apre il form per aggiungere un nuovo paziente
    /// </summary>
    [RelayCommand]
    private void AddNewPatient()
    {
        try
        {
            _logger.LogInformation("Apertura form nuovo paziente");
            
            // Naviga al form di inserimento
            _navigationService.NavigateTo<PatientFormViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore apertura form nuovo paziente");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
    }

    /// <summary>
    /// Aggiorna la lista pazienti (refresh)
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadPatientsAsync();
        _dialogService.ShowInformation($"Lista aggiornata: {TotalPatientsCount} pazienti", "Aggiornamento");
    }

    /// <summary>
    /// Mappa un'entità Patient a PatientDto con statistiche INR e TTR
    /// </summary>
    private async Task<PatientDto> MapToDtoWithStatsAsync(Data.Entities.Patient patient)
    {
        var dto = new PatientDto
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            BirthDate = patient.BirthDate,
            FiscalCode = patient.FiscalCode,
            Gender = patient.Gender?.ToString(),
            Phone = patient.Phone,
            Email = patient.Email,
            IsSlowMetabolizer = patient.IsSlowMetabolizer
        };

        // Carica indicazione attiva
        var activeIndication = patient.Indications?
            .FirstOrDefault(i => i.IsActive);
        
        if (activeIndication != null)
        {
            dto.ActiveIndication = activeIndication.IndicationType?.Description ?? "N/D";
        }

        // Carica ultimo controllo INR
        var lastControl = patient.INRControls?
            .OrderByDescending(c => c.ControlDate)
            .FirstOrDefault();

        if (lastControl != null)
        {
            dto.LastINR = lastControl.INRValue;
            dto.LastINRDate = lastControl.ControlDate;
            dto.CurrentWeeklyDose = lastControl.CurrentWeeklyDose;

            // Calcola prossimo controllo previsto
            var lastSuggestion = lastControl.DosageSuggestions?.FirstOrDefault();
            if (lastSuggestion != null)
            {
                dto.NextControlDate = lastControl.ControlDate.AddDays(lastSuggestion.NextControlDays);
            }
        }

        // Calcola TTR se ci sono abbastanza controlli
        if (patient.INRControls != null && patient.INRControls.Count >= 2 && activeIndication != null)
        {
            try
            {
                var coreControls = patient.INRControls
                    .Select(c => new Core.Models.INRControl
                    {
                        Id = c.Id,
                        ControlDate = c.ControlDate,
                        INRValue = c.INRValue,
                        CurrentWeeklyDose = c.CurrentWeeklyDose
                    })
                    .ToList();

                var ttrResult = _ttrCalculator.CalculateTTR(
                    coreControls,
                    activeIndication.TargetINRMin,
                    activeIndication.TargetINRMax);

                dto.TTRPercentage = (decimal)ttrResult.TTRPercentage;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Errore calcolo TTR per paziente {PatientId}", patient.Id);
            }
        }

        return dto;
    }
}
