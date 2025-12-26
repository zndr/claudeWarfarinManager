using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per il dialog delle preferenze applicazione
/// </summary>
public partial class PreferencesViewModel : ObservableObject
{
    private readonly IMillewinIntegrationService _millewinService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<PreferencesViewModel> _logger;

    [ObservableProperty]
    private bool _isMillewinIntegrationEnabled;

    [ObservableProperty]
    private bool _isConnectionAvailable;

    [ObservableProperty]
    private bool _isTestingConnection;

    [ObservableProperty]
    private string _connectionStatusMessage = string.Empty;

    [ObservableProperty]
    private string _connectionStatusIcon = string.Empty;

    public PreferencesViewModel(
        IMillewinIntegrationService millewinService,
        IDialogService dialogService,
        ILogger<PreferencesViewModel> logger)
    {
        _millewinService = millewinService;
        _dialogService = dialogService;
        _logger = logger;

        // Carica lo stato corrente
        LoadCurrentSettings();
    }

    private void LoadCurrentSettings()
    {
        IsMillewinIntegrationEnabled = _millewinService.IsIntegrationEnabled;
        IsConnectionAvailable = _millewinService.IsConnectionAvailable;
        UpdateConnectionStatusDisplay();
    }

    partial void OnIsMillewinIntegrationEnabledChanged(bool value)
    {
        _logger.LogInformation("Integrazione Millewin impostata a: {Enabled}", value);

        // Se viene abilitata, testa subito la connessione
        if (value)
        {
            // Non chiamare direttamente il metodo async nel changed handler
            // Lo faremo quando l'utente clicca "Salva"
            ConnectionStatusMessage = "Abilitato - Test connessione richiesto";
            ConnectionStatusIcon = "\uE946"; // Warning icon
        }
        else
        {
            IsConnectionAvailable = false;
            ConnectionStatusMessage = "Disabilitato";
            ConnectionStatusIcon = "\uE711"; // Cancel icon
        }
    }

    private void UpdateConnectionStatusDisplay()
    {
        if (!IsMillewinIntegrationEnabled)
        {
            ConnectionStatusMessage = "Integrazione disabilitata";
            ConnectionStatusIcon = "\uE711"; // Cancel icon
        }
        else if (IsConnectionAvailable)
        {
            ConnectionStatusMessage = "Connesso a Millewin";
            ConnectionStatusIcon = "\uE73E"; // Checkmark icon
        }
        else
        {
            ConnectionStatusMessage = "Non connesso - Verificare che Millewin sia in esecuzione";
            ConnectionStatusIcon = "\uE783"; // Error icon
        }
    }

    /// <summary>
    /// Testa la connessione al database Millewin
    /// </summary>
    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (IsTestingConnection) return;

        try
        {
            IsTestingConnection = true;
            ConnectionStatusMessage = "Test connessione in corso...";
            ConnectionStatusIcon = "\uE895"; // Sync icon

            _logger.LogInformation("Test manuale connessione Millewin...");

            IsConnectionAvailable = await _millewinService.TestConnectionAsync();

            UpdateConnectionStatusDisplay();

            if (IsConnectionAvailable)
            {
                _dialogService.ShowInformation(
                    "Connessione al database Millewin riuscita!\n\n" +
                    "Le funzionalità di integrazione sono ora disponibili.",
                    "Connessione Riuscita");
            }
            else
            {
                _dialogService.ShowWarning(
                    "Impossibile connettersi al database Millewin.\n\n" +
                    "Verificare che:\n" +
                    "1. Millewin sia in esecuzione\n" +
                    "2. Il database PostgreSQL (milleps) sia accessibile\n" +
                    "3. I parametri di connessione siano corretti\n\n" +
                    "Parametri utilizzati:\n" +
                    "Host: 127.0.0.1\n" +
                    "Porta: 5432\n" +
                    "Database: milleps",
                    "Connessione Fallita");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il test di connessione Millewin");
            IsConnectionAvailable = false;
            ConnectionStatusMessage = $"Errore: {ex.Message}";
            ConnectionStatusIcon = "\uE783"; // Error icon

            _dialogService.ShowError(
                $"Errore durante il test di connessione:\n{ex.Message}",
                "Errore");
        }
        finally
        {
            IsTestingConnection = false;
        }
    }

    /// <summary>
    /// Salva le preferenze e chiude il dialog
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            _logger.LogInformation("Salvataggio preferenze: MillewinIntegration={Enabled}",
                IsMillewinIntegrationEnabled);

            // Aggiorna le impostazioni nel servizio
            _millewinService.SetIntegrationEnabled(IsMillewinIntegrationEnabled);

            // Se l'integrazione è abilitata, testa la connessione
            if (IsMillewinIntegrationEnabled)
            {
                IsTestingConnection = true;
                ConnectionStatusMessage = "Verifica connessione...";

                IsConnectionAvailable = await _millewinService.TestConnectionAsync();

                IsTestingConnection = false;
                UpdateConnectionStatusDisplay();

                if (!IsConnectionAvailable)
                {
                    var result = _dialogService.ShowQuestion(
                        "L'integrazione con Millewin è stata abilitata ma la connessione al database non è disponibile.\n\n" +
                        "Le funzionalità di integrazione rimarranno disabilitate finché la connessione non sarà ripristinata.\n\n" +
                        "Vuoi comunque mantenere l'integrazione abilitata?",
                        "Connessione Non Disponibile");

                    if (result != System.Windows.MessageBoxResult.Yes)
                    {
                        IsMillewinIntegrationEnabled = false;
                        _millewinService.SetIntegrationEnabled(false);
                    }
                }
            }

            // Segnala che le impostazioni sono state salvate con successo
            // Il dialog verrà chiuso dal code-behind
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il salvataggio delle preferenze");
            _dialogService.ShowError($"Errore durante il salvataggio: {ex.Message}", "Errore");
        }
    }

    /// <summary>
    /// Indica se l'integrazione è attiva (abilitata + connessione disponibile)
    /// </summary>
    public bool IsIntegrationActive => IsMillewinIntegrationEnabled && IsConnectionAvailable;
}
