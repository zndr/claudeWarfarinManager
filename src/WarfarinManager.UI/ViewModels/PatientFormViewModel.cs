using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.Shared.Enums;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per il form di creazione/modifica paziente
/// </summary>
public partial class PatientFormViewModel : ObservableObject, Services.INavigationAware
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<PatientFormViewModel> _logger;
    private int? _patientId; // null = nuovo paziente, valore = modifica

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _pageTitle = "Nuovo Paziente";

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private DateTime _birthDate = DateTime.Today.AddYears(-50);

    [ObservableProperty]
    private string _fiscalCode = string.Empty;

    [ObservableProperty]
    private Gender? _selectedGender;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _firstNameError = string.Empty;

    [ObservableProperty]
    private string _lastNameError = string.Empty;

    [ObservableProperty]
    private string _fiscalCodeError = string.Empty;

    [ObservableProperty]
    private string _birthDateError = string.Empty;

    [ObservableProperty]
    private string _emailError = string.Empty;

    public PatientFormViewModel(
        IUnitOfWork unitOfWork,
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<PatientFormViewModel> logger)
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
            _ = LoadPatientAsync(patientId);
        }
        else
        {
            // Modalità creazione nuovo paziente
            IsEditMode = false;
            PageTitle = "Nuovo Paziente";
        }
    }

    /// <summary>
    /// Carica i dati di un paziente esistente per la modifica
    /// </summary>
    public async Task LoadPatientAsync(int patientId)
    {
        try
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
            if (patient == null)
            {
                _dialogService.ShowError("Paziente non trovato", "Errore");
                _navigationService.NavigateTo<PatientListViewModel>();
                return;
            }

            _patientId = patientId;
            IsEditMode = true;
            PageTitle = "Modifica Paziente";

            // Carica i dati nei campi
            FirstName = patient.FirstName;
            LastName = patient.LastName;
            BirthDate = patient.BirthDate;
            FiscalCode = patient.FiscalCode;
            SelectedGender = patient.Gender;
            Phone = patient.Phone ?? string.Empty;
            Email = patient.Email ?? string.Empty;
            Address = patient.Address ?? string.Empty;
            Notes = patient.Notes ?? string.Empty;

            _logger.LogInformation("Caricati dati paziente per modifica: {PatientId}", patientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il caricamento del paziente {PatientId}", patientId);
            _dialogService.ShowError($"Errore durante il caricamento:\n{ex.Message}", "Errore");
            _navigationService.NavigateTo<PatientListViewModel>();
        }
    }

    /// <summary>
    /// Lista valori Gender per ComboBox
    /// </summary>
    public Gender[] GenderValues { get; } = new[] { Gender.Male, Gender.Female, Gender.Other };

    /// <summary>
    /// Salva il paziente (nuovo o modifica)
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        try
        {
            IsSaving = true;

            // Validazione
            if (!ValidateForm())
            {
                _dialogService.ShowWarning("Correggi gli errori nel form prima di salvare.", "Validazione");
                return;
            }

            // Verifica se il codice fiscale esiste già (escludendo il paziente corrente se in modifica)
            var existingPatient = await _unitOfWork.Patients.FindAsync(p => p.FiscalCode == FiscalCode.ToUpperInvariant());
            var duplicate = existingPatient.FirstOrDefault(p => p.Id != _patientId);
            if (duplicate != null)
            {
                _dialogService.ShowError(
                    $"Esiste già un paziente con codice fiscale {FiscalCode}",
                    "Codice Fiscale Duplicato");
                return;
            }

            if (IsEditMode && _patientId.HasValue)
            {
                // Modalità modifica
                var patient = await _unitOfWork.Patients.GetByIdAsync(_patientId.Value);
                if (patient == null)
                {
                    _dialogService.ShowError("Paziente non trovato", "Errore");
                    return;
                }

                // Aggiorna i dati
                patient.FirstName = FirstName.Trim();
                patient.LastName = LastName.Trim();
                patient.BirthDate = BirthDate.Date;
                patient.FiscalCode = FiscalCode.ToUpperInvariant();
                patient.Gender = SelectedGender;
                patient.Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim();
                patient.Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim().ToLowerInvariant();
                patient.Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim();
                patient.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim();

                await _unitOfWork.Patients.UpdateAsync(patient);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Paziente aggiornato: {PatientId} - {FiscalCode} - {FullName}",
                    patient.Id, patient.FiscalCode, $"{patient.LastName} {patient.FirstName}");

                _dialogService.ShowInformation(
                    $"Paziente {patient.LastName} {patient.FirstName} aggiornato con successo!",
                    "Salvataggio Completato");
            }
            else
            {
                // Modalità creazione
                var patient = new Patient
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    BirthDate = BirthDate.Date,
                    FiscalCode = FiscalCode.ToUpperInvariant(),
                    Gender = SelectedGender,
                    Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                    Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim().ToLowerInvariant(),
                    Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
                    Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
                    IsSlowMetabolizer = false // Verrà calcolato automaticamente in base ai dosaggi
                };

                await _unitOfWork.Patients.AddAsync(patient);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Nuovo paziente creato: {FiscalCode} - {FullName}",
                    patient.FiscalCode, $"{patient.LastName} {patient.FirstName}");

                _dialogService.ShowInformation(
                    $"Paziente {patient.LastName} {patient.FirstName} creato con successo!",
                    "Salvataggio Completato");
            }

            // Torna alla lista pazienti
            _navigationService.NavigateTo<PatientListViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il salvataggio del paziente");
            _dialogService.ShowError(
                $"Errore durante il salvataggio:\n{ex.Message}",
                "Errore");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool CanSave() => !IsSaving;

    /// <summary>
    /// Annulla e torna alla lista
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        if (HasUnsavedChanges())
        {
            var result = _dialogService.ShowQuestion(
                "Ci sono modifiche non salvate. Sei sicuro di voler annullare?",
                "Conferma Annullamento");

            if (result != System.Windows.MessageBoxResult.Yes)
                return;
        }

        _navigationService.NavigateTo<PatientListViewModel>();
    }

    /// <summary>
    /// Valida tutti i campi del form
    /// </summary>
    private bool ValidateForm()
    {
        ClearErrors();
        bool isValid = true;

        // Nome obbligatorio
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            FirstNameError = "Il nome è obbligatorio";
            isValid = false;
        }
        else if (FirstName.Length < 2)
        {
            FirstNameError = "Il nome deve contenere almeno 2 caratteri";
            isValid = false;
        }

        // Cognome obbligatorio
        if (string.IsNullOrWhiteSpace(LastName))
        {
            LastNameError = "Il cognome è obbligatorio";
            isValid = false;
        }
        else if (LastName.Length < 2)
        {
            LastNameError = "Il cognome deve contenere almeno 2 caratteri";
            isValid = false;
        }

        // Codice fiscale obbligatorio e formato
        if (string.IsNullOrWhiteSpace(FiscalCode))
        {
            FiscalCodeError = "Il codice fiscale è obbligatorio";
            isValid = false;
        }
        else if (FiscalCode.Length != 16)
        {
            FiscalCodeError = "Il codice fiscale deve essere di 16 caratteri";
            isValid = false;
        }
        else if (!Regex.IsMatch(FiscalCode, @"^[A-Z]{6}[0-9]{2}[A-Z][0-9]{2}[A-Z][0-9]{3}[A-Z]$", RegexOptions.IgnoreCase))
        {
            FiscalCodeError = "Formato codice fiscale non valido";
            isValid = false;
        }

        // Data di nascita valida
        if (BirthDate > DateTime.Today)
        {
            BirthDateError = "La data di nascita non può essere nel futuro";
            isValid = false;
        }
        else if (BirthDate < DateTime.Today.AddYears(-120))
        {
            BirthDateError = "La data di nascita non è valida";
            isValid = false;
        }
        else if (CalculateAge(BirthDate) < 18)
        {
            BirthDateError = "Il paziente deve essere maggiorenne (≥18 anni)";
            isValid = false;
        }

        // Email opzionale ma se presente deve essere valida
        if (!string.IsNullOrWhiteSpace(Email))
        {
            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                EmailError = "Formato email non valido";
                isValid = false;
            }
        }

        return isValid;
    }

    private void ClearErrors()
    {
        FirstNameError = string.Empty;
        LastNameError = string.Empty;
        FiscalCodeError = string.Empty;
        BirthDateError = string.Empty;
        EmailError = string.Empty;
    }

    private bool HasUnsavedChanges()
    {
        return !string.IsNullOrWhiteSpace(FirstName) ||
               !string.IsNullOrWhiteSpace(LastName) ||
               !string.IsNullOrWhiteSpace(FiscalCode) ||
               !string.IsNullOrWhiteSpace(Phone) ||
               !string.IsNullOrWhiteSpace(Email) ||
               !string.IsNullOrWhiteSpace(Address) ||
               !string.IsNullOrWhiteSpace(Notes);
    }

    private int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;
        return age;
    }

    /// <summary>
    /// Validazione in tempo reale del codice fiscale
    /// </summary>
    partial void OnFiscalCodeChanged(string value)
    {
        // Converti in maiuscolo automaticamente
        if (!string.IsNullOrEmpty(value) && value != value.ToUpperInvariant())
        {
            FiscalCode = value.ToUpperInvariant();
        }
    }
}
