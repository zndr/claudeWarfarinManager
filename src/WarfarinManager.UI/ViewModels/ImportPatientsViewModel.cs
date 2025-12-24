using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using WarfarinManager.Core.Services;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.Shared.Enums;
using WarfarinManager.Shared.Models;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels;

public partial class ImportPatientsViewModel : ObservableObject
{
    private readonly PostgreSqlImportService _importService;
    private readonly WarfarinDbContext _dbContext;
    private readonly ILogger<ImportPatientsViewModel> _logger;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ImportPatientsCommand))]
    private string _codiceFiscaleMedico = string.Empty;

    [ObservableProperty]
    private TherapyType _selectedTherapyType = TherapyType.Warfarin;

    [ObservableProperty]
    private ObservableCollection<MillepsPatientDto> _importedPatients = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ImportPatientsCommand))]
    [NotifyCanExecuteChangedFor(nameof(SavePatientsCommand))]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasImportedData;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isConnectionSuccessful;

    public ImportPatientsViewModel(
        PostgreSqlImportService importService,
        WarfarinDbContext dbContext,
        ILogger<ImportPatientsViewModel> logger,
        IDialogService dialogService)
    {
        _importService = importService;
        _dbContext = dbContext;
        _logger = logger;
        _dialogService = dialogService;
    }

    /// <summary>
    /// Inizializza il ViewModel: esegue test automatico connessione e pre-compila codice fiscale medico
    /// </summary>
    public async Task InitializeAsync()
    {
        // Pre-compila il codice fiscale del medico dai dati salvati
        try
        {
            var doctorData = await _dbContext.DoctorData.FirstOrDefaultAsync();
            if (doctorData != null && !string.IsNullOrWhiteSpace(doctorData.FiscalCode))
            {
                CodiceFiscaleMedico = doctorData.FiscalCode;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossibile caricare il codice fiscale del medico");
        }

        // Test automatico della connessione
        IsLoading = true;
        StatusMessage = "Test connessione in corso...";

        try
        {
            IsConnectionSuccessful = await _importService.TestConnectionAsync();

            if (IsConnectionSuccessful)
            {
                StatusMessage = "✓ Connessione al database Milleps riuscita";
            }
            else
            {
                // Mostra avviso solo se il test fallisce
                StatusMessage = "✗ Impossibile connettersi al database Milleps";
                _dialogService.ShowWarning(
                    "Impossibile connettersi al database Milleps.\n\n" +
                    "Verifica che il database sia attivo e configurato correttamente.",
                    "Connessione Fallita");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il test automatico di connessione");
            StatusMessage = $"✗ Errore: {ex.Message}";
            IsConnectionSuccessful = false;

            // Mostra avviso in caso di errore
            _dialogService.ShowWarning(
                $"Errore durante il test di connessione:\n\n{ex.Message}",
                "Errore Connessione");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        IsLoading = true;
        StatusMessage = "Test connessione in corso...";

        try
        {
            IsConnectionSuccessful = await _importService.TestConnectionAsync();

            if (IsConnectionSuccessful)
            {
                StatusMessage = "✓ Connessione al database Milleps riuscita";
            }
            else
            {
                StatusMessage = "✗ Impossibile connettersi al database Milleps";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il test di connessione");
            StatusMessage = $"✗ Errore: {ex.Message}";
            IsConnectionSuccessful = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanImportPatients))]
    private async Task ImportPatientsAsync()
    {
        if (string.IsNullOrWhiteSpace(CodiceFiscaleMedico))
        {
            StatusMessage = "✗ Inserire il codice fiscale del medico";
            return;
        }

        IsLoading = true;
        StatusMessage = "Importazione pazienti in corso...";

        try
        {
            var patients = await _importService.ImportPatientsAsync(CodiceFiscaleMedico.ToUpper(), SelectedTherapyType);

            ImportedPatients.Clear();
            foreach (var patient in patients)
            {
                // Sottoscrivi all'evento PropertyChanged per aggiornare CanSavePatients
                patient.PropertyChanged += OnPatientPropertyChanged;
                ImportedPatients.Add(patient);
            }

            HasImportedData = ImportedPatients.Count > 0;

            if (HasImportedData)
            {
                StatusMessage = $"✓ Trovati {ImportedPatients.Count} pazienti";
            }
            else
            {
                StatusMessage = "⚠ Nessun paziente trovato con i criteri specificati";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'importazione dei pazienti");
            StatusMessage = $"✗ Errore: {ex.Message}";
            HasImportedData = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanImportPatients()
    {
        return !string.IsNullOrWhiteSpace(CodiceFiscaleMedico) && !IsLoading;
    }

    [RelayCommand(CanExecute = nameof(CanSavePatients))]
    private async Task SavePatientsAsync()
    {
        var patientsToSave = ImportedPatients.Where(p => p.IsSelected).ToList();

        if (patientsToSave.Count == 0)
        {
            StatusMessage = "✗ Selezionare almeno un paziente da salvare";
            return;
        }

        IsLoading = true;
        StatusMessage = "Salvataggio pazienti in corso...";

        try
        {
            int savedCount = 0;
            int skippedCount = 0;

            foreach (var dto in patientsToSave)
            {
                // Verifica se il paziente esiste già per codice fiscale
                var existingPatient = _dbContext.Patients
                    .FirstOrDefault(p => p.FiscalCode == dto.CodiceFiscale);

                if (existingPatient != null)
                {
                    _logger.LogInformation("Paziente {CF} già presente, saltato", dto.CodiceFiscale);
                    skippedCount++;
                    continue;
                }

                // Converti il sesso da stringa 'M'/'F' a enum Gender
                Gender? gender = null;
                if (!string.IsNullOrWhiteSpace(dto.Sesso))
                {
                    gender = dto.Sesso.ToUpperInvariant() switch
                    {
                        "M" => Gender.Male,
                        "F" => Gender.Female,
                        _ => null
                    };
                }

                // Crea nuovo paziente
                var newPatient = new Patient
                {
                    FirstName = dto.Nome,
                    LastName = dto.Cognome,
                    BirthDate = dto.Nascita ?? DateTime.MinValue,
                    FiscalCode = dto.CodiceFiscale,
                    Gender = gender,
                    Phone = dto.TelCell ?? string.Empty,
                    Email = dto.Email ?? string.Empty,
                    AnticoagulantType = dto.Anticoagulante,
                    TherapyStartDate = dto.DataInizio,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _dbContext.Patients.Add(newPatient);
                savedCount++;
            }

            await _dbContext.SaveChangesAsync();

            // Costruisci messaggio dettagliato sull'esito dell'importazione
            string dialogMessage;
            string dialogTitle;

            if (savedCount > 0 && skippedCount > 0)
            {
                dialogMessage = $"Importazione completata con successo!\n\n" +
                               $"✓ {savedCount} pazienti importati\n" +
                               $"⚠ {skippedCount} pazienti già presenti (non importati)";
                dialogTitle = "Importazione Completata";
                StatusMessage = $"✓ Importati {savedCount} pazienti, {skippedCount} già presenti";
            }
            else if (savedCount > 0 && skippedCount == 0)
            {
                dialogMessage = $"Importazione completata con successo!\n\n" +
                               $"✓ {savedCount} pazienti importati";
                dialogTitle = "Importazione Completata";
                StatusMessage = $"✓ {savedCount} pazienti importati con successo";
            }
            else if (savedCount == 0 && skippedCount > 0)
            {
                dialogMessage = $"Nessun paziente importato.\n\n" +
                               $"Tutti i {skippedCount} pazienti selezionati sono già presenti nel database.";
                dialogTitle = "Nessuna Importazione";
                StatusMessage = $"⚠ Tutti i {skippedCount} pazienti già presenti";
            }
            else
            {
                dialogMessage = "Nessun paziente importato.";
                dialogTitle = "Nessuna Importazione";
                StatusMessage = "⚠ Nessun paziente importato";
            }

            _logger.LogInformation("Importazione completata: {Saved} salvati, {Skipped} saltati", savedCount, skippedCount);

            // Mostra dialog con risultato importazione
            if (savedCount > 0)
            {
                _dialogService.ShowInformation(dialogMessage, dialogTitle);
            }
            else
            {
                _dialogService.ShowWarning(dialogMessage, dialogTitle);
            }

            // Rimuovi i pazienti salvati dalla lista
            foreach (var patient in patientsToSave)
            {
                ImportedPatients.Remove(patient);
            }

            HasImportedData = ImportedPatients.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il salvataggio dei pazienti");
            StatusMessage = $"✗ Errore durante il salvataggio: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanSavePatients()
    {
        return ImportedPatients.Any(p => p.IsSelected) && !IsLoading;
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var patient in ImportedPatients)
        {
            patient.IsSelected = true;
        }
        SavePatientsCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var patient in ImportedPatients)
        {
            patient.IsSelected = false;
        }
        SavePatientsCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Handler per l'evento PropertyChanged dei pazienti importati
    /// Aggiorna lo stato del comando SavePatients quando cambia IsSelected
    /// </summary>
    private void OnPatientPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MillepsPatientDto.IsSelected))
        {
            SavePatientsCommand.NotifyCanExecuteChanged();
        }
    }
}
