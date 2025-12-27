using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.Core.Services;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.Shared.Enums;
using WarfarinManager.UI.Models;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per la gestione dei farmaci concomitanti del paziente
/// </summary>
public partial class MedicationsViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInteractionCheckerService _interactionService;
    private readonly IMedicationSyncService _syncService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<MedicationsViewModel> _logger;

    [ObservableProperty]
    private int _patientId;

    private string _patientFiscalCode = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MedicationDto> _medications = new();

    [ObservableProperty]
    private MedicationDto? _selectedMedication;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _showOnlyActive = true;

    // Form nuovo farmaco
    [ObservableProperty]
    private bool _isAddingMedication;

    [ObservableProperty]
    private string _newMedicationName = string.Empty;

    [ObservableProperty]
    private string _newDosage = string.Empty;

    [ObservableProperty]
    private string _newFrequency = string.Empty;

    [ObservableProperty]
    private DateTime _newStartDate = DateTime.Today;

    // Ricerca farmaci
    [ObservableProperty]
    private ObservableCollection<string> _medicationSuggestions = new();

    [ObservableProperty]
    private bool _showSuggestions;

    // Alert interazione
    [ObservableProperty]
    private string? _interactionAlertMessage;

    [ObservableProperty]
    private string? _interactionAlertColor;

    [ObservableProperty]
    private bool _hasInteractionAlert;

    // Statistiche
    [ObservableProperty]
    private int _totalMedications;

    [ObservableProperty]
    private int _highRiskCount;

    [ObservableProperty]
    private int _moderateRiskCount;

    // Sincronizzazione Milleps
    [ObservableProperty]
    private bool _isSyncing;

    [ObservableProperty]
    private bool _canSync;

    [ObservableProperty]
    private int _millepsCount;

    public MedicationsViewModel(
        IUnitOfWork unitOfWork,
        IInteractionCheckerService interactionService,
        IMedicationSyncService syncService,
        IDialogService dialogService,
        ILogger<MedicationsViewModel> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _interactionService = interactionService ?? throw new ArgumentNullException(nameof(interactionService));
        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Verifica disponibilita sincronizzazione
        CanSync = _syncService.IsSyncAvailable;
    }

    /// <summary>
    /// Carica i farmaci del paziente
    /// </summary>
    public async Task LoadMedicationsAsync(int patientId, string? fiscalCode = null)
    {
        try
        {
            IsLoading = true;
            PatientId = patientId;

            // Salva codice fiscale per sincronizzazione
            if (!string.IsNullOrEmpty(fiscalCode))
            {
                _patientFiscalCode = fiscalCode;
            }
            else
            {
                // Recupera codice fiscale dal paziente
                var patient = await _unitOfWork.Database.Patients
                    .FirstOrDefaultAsync(p => p.Id == patientId);
                _patientFiscalCode = patient?.FiscalCode ?? string.Empty;
            }

            // Aggiorna stato sincronizzazione
            CanSync = _syncService.IsSyncAvailable && !string.IsNullOrEmpty(_patientFiscalCode);

            _logger.LogInformation("Caricamento farmaci per paziente {PatientId}", patientId);

            var query = _unitOfWork.Database.Medications
                .Where(m => m.PatientId == patientId);

            if (ShowOnlyActive)
            {
                query = query.Where(m => m.IsActive);
            }

            var medications = await query
                .OrderByDescending(m => m.IsActive)
                .ThenByDescending(m => m.InteractionLevel)
                .ThenByDescending(m => m.StartDate)
                .ToListAsync();

            Medications = new ObservableCollection<MedicationDto>(
                medications.Select(MapToDto));

            UpdateStatistics();

            _logger.LogInformation("Caricati {Count} farmaci per paziente {PatientId}",
                Medications.Count, patientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore caricamento farmaci");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Aggiorna le statistiche dei farmaci
    /// </summary>
    private void UpdateStatistics()
    {
        TotalMedications = Medications.Count(m => m.IsActive);
        HighRiskCount = Medications.Count(m => m.IsActive && m.InteractionLevel == InteractionLevel.High);
        ModerateRiskCount = Medications.Count(m => m.IsActive && m.InteractionLevel == InteractionLevel.Moderate);
        MillepsCount = Medications.Count(m => m.IsActive && m.IsFromMilleps);
    }

    /// <summary>
    /// Apre il form per aggiungere un nuovo farmaco
    /// </summary>
    [RelayCommand]
    private void StartAddMedication()
    {
        IsAddingMedication = true;
        NewMedicationName = string.Empty;
        NewDosage = string.Empty;
        NewFrequency = string.Empty;
        NewStartDate = DateTime.Today;
        ClearInteractionAlert();
    }

    /// <summary>
    /// Annulla l'aggiunta farmaco
    /// </summary>
    [RelayCommand]
    private void CancelAddMedication()
    {
        IsAddingMedication = false;
        ClearInteractionAlert();
        MedicationSuggestions.Clear();
        ShowSuggestions = false;
    }

    /// <summary>
    /// Cerca farmaci nel database durante la digitazione
    /// </summary>
    [RelayCommand]
    private async Task SearchMedicationsAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMedicationName) || NewMedicationName.Length < 2)
        {
            MedicationSuggestions.Clear();
            ShowSuggestions = false;
            ClearInteractionAlert();
            return;
        }

        try
        {
            var suggestions = await _interactionService.SearchMedicationsAsync(NewMedicationName, 8);
            
            MedicationSuggestions = new ObservableCollection<string>(suggestions);
            ShowSuggestions = suggestions.Any();

            // Verifica interazione se c'è corrispondenza esatta o parziale
            if (suggestions.Any(s => s.Equals(NewMedicationName, StringComparison.OrdinalIgnoreCase)))
            {
                await CheckInteractionAsync(NewMedicationName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore ricerca farmaci");
        }
    }

    /// <summary>
    /// Seleziona un farmaco dai suggerimenti
    /// </summary>
    [RelayCommand]
    private async Task SelectSuggestionAsync(string suggestion)
    {
        NewMedicationName = suggestion;
        ShowSuggestions = false;
        MedicationSuggestions.Clear();
        
        await CheckInteractionAsync(suggestion);
    }

    /// <summary>
    /// Verifica l'interazione del farmaco con warfarin
    /// </summary>
    private async Task CheckInteractionAsync(string medicationName)
    {
        try
        {
            var result = await _interactionService.CheckInteractionAsync(medicationName);

            if (result.HasInteraction)
            {
                HasInteractionAlert = true;
                InteractionAlertMessage = $"{result.Message}\n\n" +
                    $"📋 Gestione FCSA: {result.FCSAManagement}\n" +
                    $"📅 Controllo INR consigliato: entro {result.RecommendedINRCheckDays ?? 7} giorni";
                
                InteractionAlertColor = result.InteractionLevel switch
                {
                    InteractionLevel.High => "#FFEBEE",
                    InteractionLevel.Moderate => "#FFF3E0",
                    _ => "#E8F5E9"
                };
            }
            else
            {
                ClearInteractionAlert();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore verifica interazione");
        }
    }

    private void ClearInteractionAlert()
    {
        HasInteractionAlert = false;
        InteractionAlertMessage = null;
        InteractionAlertColor = null;
    }

    /// <summary>
    /// Salva il nuovo farmaco
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveMedication))]
    private async Task SaveMedicationAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NewMedicationName))
            {
                _dialogService.ShowError("Il nome del farmaco è obbligatorio", "Errore");
                return;
            }

            IsLoading = true;

            // Verifica interazione
            var interactionResult = await _interactionService.CheckInteractionAsync(NewMedicationName);

            // Crea entity
            var medication = new Medication
            {
                PatientId = PatientId,
                MedicationName = NewMedicationName.Trim(),
                Dosage = string.IsNullOrWhiteSpace(NewDosage) ? null : NewDosage.Trim(),
                Frequency = string.IsNullOrWhiteSpace(NewFrequency) ? null : NewFrequency.Trim(),
                StartDate = NewStartDate,
                IsActive = true,
                InteractionLevel = interactionResult.InteractionLevel,
                InteractionDetails = interactionResult.HasInteraction
                    ? System.Text.Json.JsonSerializer.Serialize(new
                    {
                        interactionResult.InteractionEffect,
                        interactionResult.Mechanism,
                        interactionResult.FCSAManagement,
                        interactionResult.ACCPManagement,
                        interactionResult.RecommendedINRCheckDays,
                        interactionResult.OddsRatio
                    })
                    : null
            };

            await _unitOfWork.Database.Medications.AddAsync(medication);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Farmaco {MedicationName} aggiunto per paziente {PatientId}",
                medication.MedicationName, PatientId);

            // Mostra messaggio successo con alert interazione se necessario
            var successMessage = $"Farmaco '{medication.MedicationName}' aggiunto con successo.";
            
            if (interactionResult.HasInteraction)
            {
                successMessage += $"\n\n⚠️ ATTENZIONE: Interazione con Warfarin rilevata!\n" +
                    $"Livello: {interactionResult.InteractionLevel}\n" +
                    $"Controllo INR consigliato entro {interactionResult.RecommendedINRCheckDays ?? 7} giorni.";
            }

            _dialogService.ShowInformation(successMessage, "Farmaco Aggiunto");

            // Reset form e ricarica lista
            IsAddingMedication = false;
            ClearInteractionAlert();
            await LoadMedicationsAsync(PatientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore salvataggio farmaco");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanSaveMedication() => !string.IsNullOrWhiteSpace(NewMedicationName);

    /// <summary>
    /// Sospende il farmaco selezionato
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSuspendMedication))]
    private async Task SuspendMedicationAsync()
    {
        if (SelectedMedication == null) return;

        try
        {
            var confirm = _dialogService.ShowQuestion(
                $"Vuoi sospendere il farmaco '{SelectedMedication.MedicationName}'?\n\n" +
                $"Data inizio: {SelectedMedication.StartDate:dd/MM/yyyy}",
                "Conferma Sospensione");

            if (confirm != System.Windows.MessageBoxResult.Yes)
                return;

            var medication = await _unitOfWork.Database.Medications
                .FirstOrDefaultAsync(m => m.Id == SelectedMedication.Id);

            if (medication == null)
            {
                _dialogService.ShowError("Farmaco non trovato", "Errore");
                return;
            }

            medication.EndDate = DateTime.Today;
            medication.IsActive = false;

            await _unitOfWork.SaveChangesAsync();

            // Alert se farmaco con interazione
            if (medication.InteractionLevel != InteractionLevel.None)
            {
                _dialogService.ShowInformation(
                    $"Farmaco '{medication.MedicationName}' sospeso.\n\n" +
                    "⚠️ NOTA: Il farmaco aveva interazione con Warfarin.\n" +
                    "Considerare controllo INR entro 5-7 giorni per verificare eventuale " +
                    "necessità di aggiustamento dose.",
                    "Farmaco Sospeso");
            }
            else
            {
                _dialogService.ShowInformation("Farmaco sospeso con successo", "Successo");
            }

            await LoadMedicationsAsync(PatientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore sospensione farmaco");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
    }

    private bool CanSuspendMedication() => SelectedMedication != null && SelectedMedication.IsActive;

    /// <summary>
    /// Riattiva il farmaco selezionato
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanReactivateMedication))]
    private async Task ReactivateMedicationAsync()
    {
        if (SelectedMedication == null) return;

        try
        {
            var confirm = _dialogService.ShowQuestion(
                $"Vuoi riattivare il farmaco '{SelectedMedication.MedicationName}'?",
                "Conferma Riattivazione");

            if (confirm != System.Windows.MessageBoxResult.Yes)
                return;

            var medication = await _unitOfWork.Database.Medications
                .FirstOrDefaultAsync(m => m.Id == SelectedMedication.Id);

            if (medication == null)
            {
                _dialogService.ShowError("Farmaco non trovato", "Errore");
                return;
            }

            medication.EndDate = null;
            medication.IsActive = true;

            await _unitOfWork.SaveChangesAsync();

            // Alert se farmaco con interazione
            if (medication.InteractionLevel != InteractionLevel.None)
            {
                var interactionResult = await _interactionService.CheckInteractionAsync(medication.MedicationName);
                
                _dialogService.ShowInformation(
                    $"Farmaco '{medication.MedicationName}' riattivato.\n\n" +
                    $"⚠️ ATTENZIONE: Interazione con Warfarin!\n" +
                    $"Livello: {interactionResult.InteractionLevel}\n" +
                    $"Controllo INR consigliato entro {interactionResult.RecommendedINRCheckDays ?? 7} giorni.",
                    "Farmaco Riattivato");
            }
            else
            {
                _dialogService.ShowInformation("Farmaco riattivato con successo", "Successo");
            }

            await LoadMedicationsAsync(PatientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore riattivazione farmaco");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
    }

    private bool CanReactivateMedication() => SelectedMedication != null && !SelectedMedication.IsActive;

    /// <summary>
    /// Elimina definitivamente il farmaco
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteMedication))]
    private async Task DeleteMedicationAsync()
    {
        if (SelectedMedication == null) return;

        try
        {
            var confirm = _dialogService.ShowQuestion(
                $"Vuoi eliminare DEFINITIVAMENTE il farmaco '{SelectedMedication.MedicationName}'?\n\n" +
                "⚠️ Questa azione non può essere annullata.",
                "Conferma Eliminazione");

            if (confirm != System.Windows.MessageBoxResult.Yes)
                return;

            var medication = await _unitOfWork.Database.Medications
                .FirstOrDefaultAsync(m => m.Id == SelectedMedication.Id);

            if (medication == null)
            {
                _dialogService.ShowError("Farmaco non trovato", "Errore");
                return;
            }

            _unitOfWork.Database.Medications.Remove(medication);
            await _unitOfWork.SaveChangesAsync();

            _dialogService.ShowInformation("Farmaco eliminato con successo", "Successo");
            await LoadMedicationsAsync(PatientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore eliminazione farmaco");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
    }

    private bool CanDeleteMedication() => SelectedMedication != null;

    /// <summary>
    /// Mostra dettagli interazione del farmaco selezionato
    /// </summary>
    [RelayCommand]
    private async Task ShowInteractionDetailsAsync()
    {
        if (SelectedMedication == null) return;

        try
        {
            var result = await _interactionService.CheckInteractionAsync(SelectedMedication.MedicationName);

            if (!result.HasInteraction)
            {
                _dialogService.ShowInformation(
                    $"Nessuna interazione nota tra '{SelectedMedication.MedicationName}' e Warfarin.",
                    "Interazioni Farmacologiche");
                return;
            }

            var message = $"💊 {result.MedicationName}\n" +
                $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n" +
                $"⚠️ Livello Rischio: {result.InteractionLevel}\n\n" +
                $"📈 Effetto: {result.InteractionEffect}\n" +
                (result.OddsRatio.HasValue ? $"   OR Sanguinamento: {result.OddsRatio:F2}\n" : "") +
                $"\n🔬 Meccanismo:\n   {result.Mechanism}\n" +
                $"\n📋 Gestione FCSA-SIMG:\n   {result.FCSAManagement}\n" +
                (!string.IsNullOrEmpty(result.ACCPManagement) 
                    ? $"\n📋 Gestione ACCP:\n   {result.ACCPManagement}\n" 
                    : "") +
                $"\n📅 Controllo INR consigliato: entro {result.RecommendedINRCheckDays ?? 7} giorni";

            _dialogService.ShowInformation(message, "Dettagli Interazione");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore recupero dettagli interazione");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
    }

    /// <summary>
    /// Sincronizza i farmaci da Millewin
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSyncFromMilleps))]
    private async Task SyncFromMillepsAsync()
    {
        if (string.IsNullOrEmpty(_patientFiscalCode))
        {
            _dialogService.ShowError(
                "Codice fiscale del paziente non disponibile per la sincronizzazione.",
                "Sincronizzazione non disponibile");
            return;
        }

        try
        {
            IsSyncing = true;

            _logger.LogInformation(
                "Avvio sincronizzazione farmaci da Millewin per paziente {PatientId}",
                PatientId);

            var result = await _syncService.SyncMedicationsAsync(PatientId, _patientFiscalCode);

            if (!result.Success)
            {
                _dialogService.ShowError(
                    $"Sincronizzazione non riuscita:\n{result.ErrorMessage}",
                    "Errore Sincronizzazione");
                return;
            }

            // Ricarica la lista farmaci
            await LoadMedicationsAsync(PatientId, _patientFiscalCode);

            // Mostra messaggio risultato
            if (result.HasChanges)
            {
                var message = "Sincronizzazione completata:\n\n";

                if (result.Added > 0)
                    message += $"  + {result.Added} farmaci aggiunti\n";
                if (result.Updated > 0)
                    message += $"  ~ {result.Updated} farmaci aggiornati\n";
                if (result.Deactivated > 0)
                    message += $"  - {result.Deactivated} farmaci disattivati\n";

                if (result.NewInteractions > 0)
                {
                    message += $"\n  Rilevate {result.NewInteractions} nuove interazioni con Warfarin.";
                }

                _dialogService.ShowInformation(message, "Sincronizzazione Millewin");
            }
            else
            {
                _dialogService.ShowInformation(
                    "Nessuna modifica necessaria.\nI farmaci sono gia sincronizzati con Millewin.",
                    "Sincronizzazione Millewin");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante sincronizzazione farmaci da Millewin");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore Sincronizzazione");
        }
        finally
        {
            IsSyncing = false;
        }
    }

    private bool CanSyncFromMilleps() => CanSync && !IsSyncing && !string.IsNullOrEmpty(_patientFiscalCode);

    /// <summary>
    /// Toggle visualizzazione solo farmaci attivi
    /// </summary>
    partial void OnShowOnlyActiveChanged(bool value)
    {
        if (PatientId > 0)
        {
            _ = LoadMedicationsAsync(PatientId);
        }
    }

    partial void OnSelectedMedicationChanged(MedicationDto? value)
    {
        SuspendMedicationCommand.NotifyCanExecuteChanged();
        ReactivateMedicationCommand.NotifyCanExecuteChanged();
        DeleteMedicationCommand.NotifyCanExecuteChanged();
    }

    partial void OnNewMedicationNameChanged(string value)
    {
        SaveMedicationCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Mappa entity a DTO
    /// </summary>
    private static MedicationDto MapToDto(Medication medication)
    {
        return new MedicationDto
        {
            Id = medication.Id,
            PatientId = medication.PatientId,
            MedicationName = medication.MedicationName,
            AtcCode = medication.AtcCode,
            ActiveIngredient = medication.ActiveIngredient,
            Source = medication.Source,
            Dosage = medication.Dosage,
            Frequency = medication.Frequency,
            StartDate = medication.StartDate,
            EndDate = medication.EndDate,
            IsActive = medication.IsActive,
            InteractionLevel = medication.InteractionLevel,
            InteractionDetails = medication.InteractionDetails
        };
    }
}
