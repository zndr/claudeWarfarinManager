using Microsoft.Extensions.Logging;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.UI.Properties;

namespace WarfarinManager.UI.Services;

/// <summary>
/// Servizio per la gestione dell'integrazione con Millewin.
/// Controlla lo stato dell'integrazione e la disponibilità della connessione al database milleps.
/// </summary>
public class MillewinIntegrationService : IMillewinIntegrationService
{
    private readonly IMillepsDataService _millepsDataService;
    private readonly ILogger<MillewinIntegrationService> _logger;
    private bool _isConnectionAvailable;

    public MillewinIntegrationService(
        IMillepsDataService millepsDataService,
        ILogger<MillewinIntegrationService> logger)
    {
        _millepsDataService = millepsDataService;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsIntegrationEnabled => Settings.Default.MillewinIntegrationEnabled;

    /// <inheritdoc />
    public bool IsConnectionAvailable => _isConnectionAvailable;

    /// <inheritdoc />
    public bool IsIntegrationActive => IsIntegrationEnabled && IsConnectionAvailable;

    /// <inheritdoc />
    public event EventHandler<bool>? IntegrationStateChanged;

    /// <inheritdoc />
    public void SetIntegrationEnabled(bool enabled)
    {
        var previousState = IsIntegrationActive;

        Settings.Default.MillewinIntegrationEnabled = enabled;
        Settings.Default.Save();

        _logger.LogInformation("Integrazione Millewin {State}", enabled ? "abilitata" : "disabilitata");

        // Se l'integrazione viene disabilitata, resetta anche lo stato della connessione
        if (!enabled)
        {
            _isConnectionAvailable = false;
        }

        // Notifica se lo stato attivo è cambiato
        var currentState = IsIntegrationActive;
        if (previousState != currentState)
        {
            IntegrationStateChanged?.Invoke(this, currentState);
        }
    }

    /// <inheritdoc />
    public async Task<bool> TestConnectionAsync()
    {
        var previousState = IsIntegrationActive;

        try
        {
            _logger.LogInformation("Test connessione database Millewin...");
            _isConnectionAvailable = await _millepsDataService.TestConnectionAsync();

            if (_isConnectionAvailable)
            {
                _logger.LogInformation("Connessione al database Millewin riuscita");
            }
            else
            {
                _logger.LogWarning("Connessione al database Millewin fallita");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il test di connessione al database Millewin");
            _isConnectionAvailable = false;
        }

        // Notifica se lo stato attivo è cambiato
        var currentState = IsIntegrationActive;
        if (previousState != currentState)
        {
            IntegrationStateChanged?.Invoke(this, currentState);
        }

        return _isConnectionAvailable;
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        if (IsIntegrationEnabled)
        {
            _logger.LogInformation("Integrazione Millewin abilitata, test connessione iniziale...");
            await TestConnectionAsync();
        }
        else
        {
            _logger.LogInformation("Integrazione Millewin disabilitata");
        }
    }
}
