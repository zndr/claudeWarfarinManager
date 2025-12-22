using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WarfarinManager.Core.Models;
using WarfarinManager.Core.Services;
using WarfarinManager.UI.Views.Dialogs;

namespace WarfarinManager.UI.Services;

/// <summary>
/// Servizio UI per gestire le notifiche di aggiornamento
/// </summary>
public class UpdateNotificationService : IDisposable
{
    private readonly ILogger<UpdateNotificationService> _logger;
    private readonly IUpdateCheckerService _updateChecker;
    private readonly IConfiguration _configuration;
    private readonly string _currentVersion;
    private readonly DispatcherTimer _timer;
    private bool _disposed;

    public UpdateNotificationService(
        ILogger<UpdateNotificationService> logger,
        IUpdateCheckerService updateChecker,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _updateChecker = updateChecker ?? throw new ArgumentNullException(nameof(updateChecker));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // Ottieni la versione corrente dall'assembly principale (entry point)
        // Usa GetEntryAssembly per ottenere la versione dell'applicazione principale,
        // non dell'assembly corrente che potrebbe avere versione diversa
        var assembly = System.Reflection.Assembly.GetEntryAssembly()
                       ?? System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        _currentVersion = version?.ToString() ?? "1.0.0.0";

        _logger.LogDebug("Versione corrente dell'applicazione: {CurrentVersion}", _currentVersion);

        // Configura il timer per il controllo periodico
        _timer = new DispatcherTimer();
        _timer.Tick += async (s, e) => await CheckForUpdatesAsync(false);

        // Leggi l'intervallo dalla configurazione (default: 24 ore)
        var intervalHours = _configuration.GetValue("UpdateChecker:CheckIntervalHours", 24);
        _timer.Interval = TimeSpan.FromHours(intervalHours);
    }

    /// <summary>
    /// Avvia il servizio di controllo periodico degli aggiornamenti
    /// </summary>
    public void Start()
    {
        var enabled = _configuration.GetValue("UpdateChecker:Enabled", true);
        if (!enabled)
        {
            _logger.LogInformation("Controllo aggiornamenti disabilitato");
            return;
        }

        var checkOnStartup = _configuration.GetValue("UpdateChecker:CheckOnStartup", true);
        if (checkOnStartup)
        {
            _logger.LogInformation("Avvio controllo aggiornamenti all'avvio (modalità silenziosa)");
            // Passa false per non mostrare messaggi quando non ci sono aggiornamenti
            Task.Run(() => CheckForUpdatesAsync(false));
        }

        _timer.Start();
        _logger.LogInformation("Servizio controllo aggiornamenti avviato. Intervallo: {Interval}", _timer.Interval);
    }

    /// <summary>
    /// Ferma il servizio di controllo periodico
    /// </summary>
    public void Stop()
    {
        _timer.Stop();
        _logger.LogInformation("Servizio controllo aggiornamenti fermato");
    }

    /// <summary>
    /// Controlla manualmente se sono disponibili aggiornamenti
    /// </summary>
    /// <param name="showNoUpdateMessage">Se true, mostra un messaggio anche se non ci sono aggiornamenti</param>
    public async Task CheckForUpdatesAsync(bool showNoUpdateMessage = false)
    {
        try
        {
            _logger.LogInformation("Controllo aggiornamenti in corso...");

            var updateInfo = await _updateChecker.CheckForUpdateAsync(_currentVersion);

            if (updateInfo != null)
            {
                _logger.LogInformation(
                    "Nuova versione disponibile: {Version}. Versione corrente: {CurrentVersion}",
                    updateInfo.Version,
                    _currentVersion);

                // Mostra la finestra di notifica nel thread UI SOLO quando c'è un aggiornamento
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ShowUpdateNotification(updateInfo);
                });
            }
            else
            {
                _logger.LogInformation("Nessun aggiornamento disponibile. Versione corrente: {CurrentVersion}", _currentVersion);

                // NON mostrare alcun messaggio quando non ci sono aggiornamenti (comportamento silenzioso)
                // Il messaggio viene mostrato SOLO se l'utente controlla manualmente (showNoUpdateMessage = true)
                if (showNoUpdateMessage)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MessageBox.Show(
                            $"Stai utilizzando l'ultima versione disponibile ({_currentVersion})",
                            "Nessun aggiornamento",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il controllo degli aggiornamenti");

            // NON mostrare errori durante il controllo automatico all'avvio
            // Gli errori vengono mostrati SOLO per controlli manuali
            if (showNoUpdateMessage)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(
                        "Impossibile controllare gli aggiornamenti.\nVerificare la connessione internet.",
                        "Errore",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                });
            }
        }
    }

    private void ShowUpdateNotification(UpdateInfo updateInfo)
    {
        try
        {
            var dialog = new UpdateNotificationWindow(updateInfo, _currentVersion);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la visualizzazione della finestra di aggiornamento");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _timer?.Stop();
        _disposed = true;
    }
}
