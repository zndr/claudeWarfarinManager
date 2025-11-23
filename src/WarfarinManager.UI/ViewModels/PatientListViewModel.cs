using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
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
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<PatientListViewModel> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Carica tutti i pazienti dal database
    /// </summary>
    [RelayCommand]
    private async Task LoadPatientsAsync()
    {
        try
        {
            IsLoading = true;
            _logger.LogInformation("Caricamento lista pazienti...");

            var patients = await _unitOfWork.Patients.GetAllAsync();
            
            var patientDtos = patients
                .Select(p => MapToDto(p))
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToList();

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
    /// Aggiorna il CanExecute quando cambia la selezione
    /// </summary>
    partial void OnSelectedPatientChanged(PatientDto? value)
    {
        OpenPatientDetailsCommand.NotifyCanExecuteChanged();
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
    /// Mappa un'entità Patient a PatientDto
    /// </summary>
    private PatientDto MapToDto(Data.Entities.Patient patient)
    {
        // Per ora mappiamo solo i dati base
        // TODO: Aggiungere caricamento indicazioni attive, ultimo INR, TTR
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
            
            // TODO: Caricare da database
            ActiveIndication = null,
            LastINR = null,
            LastINRDate = null,
            TTRPercentage = null,
            NextControlDate = null
        };
    }
}
