using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using WarfarinManager.Core.Services;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.Shared.Models;

namespace WarfarinManager.UI.ViewModels;

public partial class ImportPatientsViewModel : ObservableObject
{
    private readonly PostgreSqlImportService _importService;
    private readonly WarfarinDbContext _dbContext;
    private readonly ILogger<ImportPatientsViewModel> _logger;

    [ObservableProperty]
    private string _codiceFiscaleMedico = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MillepsPatientDto> _importedPatients = new();

    [ObservableProperty]
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
        ILogger<ImportPatientsViewModel> logger)
    {
        _importService = importService;
        _dbContext = dbContext;
        _logger = logger;
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
            var patients = await _importService.ImportPatientsAsync(CodiceFiscaleMedico);

            ImportedPatients.Clear();
            foreach (var patient in patients)
            {
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

                // Crea nuovo paziente
                var newPatient = new Patient
                {
                    FirstName = dto.Nome,
                    LastName = dto.Cognome,
                    BirthDate = dto.Nascita ?? DateTime.MinValue,
                    FiscalCode = dto.CodiceFiscale,
                    Phone = dto.TelCell ?? string.Empty,
                    Email = dto.Email ?? string.Empty,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _dbContext.Patients.Add(newPatient);
                savedCount++;
            }

            await _dbContext.SaveChangesAsync();

            StatusMessage = $"✓ Salvati {savedCount} pazienti";
            if (skippedCount > 0)
            {
                StatusMessage += $", {skippedCount} già presenti";
            }

            _logger.LogInformation("Importazione completata: {Saved} salvati, {Skipped} saltati", savedCount, skippedCount);

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
}
