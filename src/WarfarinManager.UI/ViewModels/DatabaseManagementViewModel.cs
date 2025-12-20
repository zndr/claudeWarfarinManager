using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per la gestione del database
/// </summary>
public partial class DatabaseManagementViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<DatabaseManagementViewModel> _logger;

    [ObservableProperty]
    private string _databasePath = string.Empty;

    [ObservableProperty]
    private double _databaseSizeMB;

    [ObservableProperty]
    private int _totalPatients;

    [ObservableProperty]
    private int _totalINRControls;

    [ObservableProperty]
    private int _totalIndications;

    [ObservableProperty]
    private int _totalAdverseEvents;

    [ObservableProperty]
    private DateTime? _lastBackupDate;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public DatabaseManagementViewModel(
        IDatabaseService databaseService,
        IDialogService dialogService,
        ILogger<DatabaseManagementViewModel> logger)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Carica le statistiche del database
    /// </summary>
    [RelayCommand]
    private async Task LoadStatisticsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Caricamento statistiche...";

            DatabasePath = _databaseService.DatabasePath;

            var stats = await _databaseService.GetStatisticsAsync();

            TotalPatients = stats.TotalPatients;
            TotalINRControls = stats.TotalINRControls;
            TotalIndications = stats.TotalIndications;
            TotalAdverseEvents = stats.TotalAdverseEvents;
            DatabaseSizeMB = stats.DatabaseSizeMB;
            LastBackupDate = stats.LastBackupDate;

            StatusMessage = "Statistiche aggiornate";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel caricamento statistiche");
            _dialogService.ShowError($"Errore nel caricamento statistiche:\n{ex.Message}", "Errore");
            StatusMessage = "Errore nel caricamento";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Esegue il backup del database
    /// </summary>
    [RelayCommand]
    private async Task BackupAsync()
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Title = "Salva backup database",
                Filter = "Database SQLite (*.db)|*.db|Tutti i file (*.*)|*.*",
                FileName = $"warfarin_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db",
                DefaultExt = ".db"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            IsLoading = true;
            StatusMessage = "Backup in corso...";

            var success = await _databaseService.BackupDatabaseAsync(saveDialog.FileName);

            if (success)
            {
                _dialogService.ShowInformation(
                    $"Backup completato con successo!\n\nFile salvato in:\n{saveDialog.FileName}",
                    "Backup completato");

                StatusMessage = "Backup completato";
                LastBackupDate = DateTime.Now;
            }
            else
            {
                _dialogService.ShowError(
                    "Errore durante il backup del database.\nControllare i log per i dettagli.",
                    "Errore backup");

                StatusMessage = "Backup fallito";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il backup");
            _dialogService.ShowError($"Errore durante il backup:\n{ex.Message}", "Errore");
            StatusMessage = "Errore backup";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Ripristina il database da un backup
    /// </summary>
    [RelayCommand]
    private async Task RestoreAsync()
    {
        try
        {
            // Conferma operazione
            var confirm = _dialogService.ShowConfirmation(
                "ATTENZIONE: Il ripristino sostituirà completamente il database corrente.\n\n" +
                "Verrà creato un backup di emergenza del database attuale prima del ripristino.\n\n" +
                "Continuare?",
                "Conferma ripristino");

            if (!confirm)
                return;

            var openDialog = new OpenFileDialog
            {
                Title = "Seleziona file di backup",
                Filter = "Database SQLite (*.db)|*.db|Tutti i file (*.*)|*.*",
                CheckFileExists = true
            };

            if (openDialog.ShowDialog() != true)
                return;

            IsLoading = true;
            StatusMessage = "Ripristino in corso...";

            var success = await _databaseService.RestoreDatabaseAsync(openDialog.FileName);

            if (success)
            {
                _dialogService.ShowInformation(
                    "Ripristino completato con successo!\n\n" +
                    "L'applicazione verrà riavviata per applicare le modifiche.",
                    "Ripristino completato");

                StatusMessage = "Ripristino completato";

                // Richiedi riavvio applicazione
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                _dialogService.ShowError(
                    "Errore durante il ripristino del database.\n" +
                    "Il database corrente non è stato modificato.",
                    "Errore ripristino");

                StatusMessage = "Ripristino fallito";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il ripristino");
            _dialogService.ShowError($"Errore durante il ripristino:\n{ex.Message}", "Errore");
            StatusMessage = "Errore ripristino";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Compatta il database
    /// </summary>
    [RelayCommand]
    private async Task CompactAsync()
    {
        try
        {
            var confirm = _dialogService.ShowConfirmation(
                "La compattazione ottimizza il database riducendone le dimensioni.\n\n" +
                "L'operazione potrebbe richiedere alcuni secondi.\n\n" +
                "Continuare?",
                "Conferma compattazione");

            if (!confirm)
                return;

            IsLoading = true;
            StatusMessage = "Compattazione in corso...";

            var sizeBefore = DatabaseSizeMB;

            var success = await _databaseService.CompactDatabaseAsync();

            if (success)
            {
                // Ricarica statistiche
                await LoadStatisticsAsync();

                var sizeReduced = sizeBefore - DatabaseSizeMB;

                _dialogService.ShowInformation(
                    $"Compattazione completata con successo!\n\n" +
                    $"Dimensione prima: {sizeBefore:F2} MB\n" +
                    $"Dimensione dopo: {DatabaseSizeMB:F2} MB\n" +
                    $"Spazio recuperato: {sizeReduced:F2} MB",
                    "Compattazione completata");

                StatusMessage = "Compattazione completata";
            }
            else
            {
                _dialogService.ShowError(
                    "Errore durante la compattazione del database.",
                    "Errore compattazione");

                StatusMessage = "Compattazione fallita";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la compattazione");
            _dialogService.ShowError($"Errore durante la compattazione:\n{ex.Message}", "Errore");
            StatusMessage = "Errore compattazione";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Verifica l'integrità del database
    /// </summary>
    [RelayCommand]
    private async Task CheckIntegrityAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Verifica integrità in corso...";

            var result = await _databaseService.CheckIntegrityAsync();

            if (result.IsValid)
            {
                _dialogService.ShowInformation(
                    "Verifica completata con successo!\n\n" +
                    "Il database è integro e non presenta errori.",
                    "Verifica integrità");

                StatusMessage = "Database integro";
            }
            else
            {
                var errors = string.Join("\n", result.Errors);
                _dialogService.ShowWarning(
                    $"Verifica completata: trovati errori!\n\n" +
                    $"Errori:\n{errors}\n\n" +
                    $"Si consiglia di ripristinare da un backup.",
                    "Problemi di integrità");

                StatusMessage = "Database con errori";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la verifica integrità");
            _dialogService.ShowError($"Errore durante la verifica:\n{ex.Message}", "Errore");
            StatusMessage = "Errore verifica";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Apre la cartella del database in Esplora risorse
    /// </summary>
    [RelayCommand]
    private void OpenDatabaseFolder()
    {
        try
        {
            if (!string.IsNullOrEmpty(DatabasePath) && File.Exists(DatabasePath))
            {
                var folder = Path.GetDirectoryName(DatabasePath);
                if (!string.IsNullOrEmpty(folder))
                {
                    System.Diagnostics.Process.Start("explorer.exe", folder);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nell'apertura cartella database");
            _dialogService.ShowError($"Impossibile aprire la cartella:\n{ex.Message}", "Errore");
        }
    }
}
