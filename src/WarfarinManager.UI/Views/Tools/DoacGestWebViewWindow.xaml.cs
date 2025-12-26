using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace WarfarinManager.UI.Views.Tools
{
    /// <summary>
    /// Finestra WebView2 per DoacGest Expert - Modalità simulazione paziente ipotetico
    /// </summary>
    public partial class DoacGestWebViewWindow : Window
    {
        private readonly ILogger<DoacGestWebViewWindow> _logger;
        private string? _modulePath;

        public DoacGestWebViewWindow(ILogger<DoacGestWebViewWindow> logger)
        {
            InitializeComponent();
            _logger = logger;
            Loaded += OnWindowLoaded;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Path al modulo DoacGest React compilato
                // IMPORTANTE: Questa cartella deve contenere i file dist/ del progetto React
                _modulePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Modules",
                    "DoacGest"
                );

                if (!Directory.Exists(_modulePath))
                {
                    _logger.LogError("Modulo DoacGest non trovato in: {ModulePath}", _modulePath);
                    MessageBox.Show(
                        $"Modulo DoacGest non trovato in:\n{_modulePath}\n\n" +
                        "Verifica che la cartella 'Modules/DoacGest' contenga i file compilati del progetto React.\n\n" +
                        "Per istruzioni sulla compilazione e installazione, consulta INTEGRATION_TAOGEST.md",
                        "Modulo non trovato",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    Close();
                    return;
                }

                // Verifica esistenza index.html
                var indexPath = Path.Combine(_modulePath, "index.html");
                if (!File.Exists(indexPath))
                {
                    _logger.LogError("File index.html non trovato in: {IndexPath}", indexPath);
                    MessageBox.Show(
                        $"File index.html non trovato in:\n{indexPath}\n\n" +
                        "Verifica di aver copiato correttamente la cartella dist/ del progetto React.",
                        "File mancante",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    Close();
                    return;
                }

                _logger.LogInformation("Inizializzazione WebView2 per DoacGest...");

                // Inizializza WebView2
                await DoacGestWebView.EnsureCoreWebView2Async(null);

                _logger.LogInformation("WebView2 inizializzato con successo");

                // Configura host virtuale per servire i file locali
                DoacGestWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "doacgest.local",
                    _modulePath,
                    CoreWebView2HostResourceAccessKind.Allow
                );

                _logger.LogInformation("Virtual host configurato: doacgest.local -> {ModulePath}", _modulePath);

                // Abilita DevTools in modalità debug
#if DEBUG
                DoacGestWebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
#else
                DoacGestWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
#endif

                // Disabilita context menu in produzione
                DoacGestWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

                // Listener messaggi da React
                DoacGestWebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

                // Listener navigazione completata
                DoacGestWebView.CoreWebView2.NavigationCompleted += (s, args) =>
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                    _logger.LogInformation("Navigazione completata. Success: {Success}", args.IsSuccess);

                    if (!args.IsSuccess)
                    {
                        _logger.LogError("Errore navigazione WebView2. WebErrorStatus: {ErrorStatus}", args.WebErrorStatus);
                        MessageBox.Show(
                            $"Errore durante il caricamento di DoacGest.\nErrore: {args.WebErrorStatus}",
                            "Errore",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                };

                // Naviga alla app React
                var appUrl = "https://doacgest.local/index.html";
                _logger.LogInformation("Navigazione verso: {AppUrl}", appUrl);
                DoacGestWebView.CoreWebView2.Navigate(appUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'inizializzazione di DoacGest WebView2");
                MessageBox.Show(
                    $"Errore inizializzazione DoacGest:\n{ex.Message}\n\n" +
                    "Verifica che WebView2 Runtime sia installato.\n" +
                    "Scarica da: https://developer.microsoft.com/microsoft-edge/webview2/",
                    "Errore",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Close();
            }
        }

        // ========== COMUNICAZIONE DA REACT ==========

        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var json = e.WebMessageAsJson;
                _logger.LogDebug("Messaggio ricevuto da React: {Json}", json);

                var message = JsonSerializer.Deserialize<WebMessage>(json);

                switch (message?.Type)
                {
                    case "MODULE_READY":
                        OnModuleReady();
                        break;

                    case "SAVE_SIMULATION":
                        SaveSimulation(message.Payload);
                        break;

                    case "EXPORT_REPORT":
                        ExportReport(message.Payload);
                        break;

                    case "PRINT_REQUEST":
                        PrintReport();
                        break;

                    case "SHOW_NOTIFICATION":
                        ShowNotification(message.Payload);
                        break;

                    case "OPEN_DEVTOOLS":
#if DEBUG
                        DoacGestWebView.CoreWebView2.OpenDevToolsWindow();
#endif
                        break;

                    default:
                        _logger.LogWarning("Tipo messaggio sconosciuto: {Type}", message?.Type);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'elaborazione del messaggio WebView");
                MessageBox.Show(
                    $"Errore comunicazione con DoacGest:\n{ex.Message}",
                    "Errore",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // ========== HANDLERS ==========

        private void OnModuleReady()
        {
            _logger.LogInformation("Modulo React pronto");
            SendToReact("MODULE_INITIALIZED", new { version = "1.0.0", mode = "simulation" });
        }

        private void SaveSimulation(object? payload)
        {
            try
            {
                if (payload == null) return;

                // TODO: Implementa salvataggio simulazione
                // Opzioni:
                // 1. Salva in un file JSON temporaneo
                // 2. Permetti di creare un nuovo paziente dal risultato della simulazione
                // 3. Esporta come report PDF

                var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
                _logger.LogInformation("Richiesta salvataggio simulazione: {Payload}", jsonPayload);

                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var savePath = Path.Combine(documentsPath, "TaoGEST", "Simulazioni");
                Directory.CreateDirectory(savePath);

                var filename = $"DoacGest_Simulation_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var fullPath = Path.Combine(savePath, filename);

                File.WriteAllText(fullPath, jsonPayload);

                SendNotification($"Simulazione salvata in:\n{fullPath}", "success");
                _logger.LogInformation("Simulazione salvata: {FilePath}", fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il salvataggio della simulazione");
                SendNotification($"Errore salvataggio: {ex.Message}", "error");
            }
        }

        private void ExportReport(object? payload)
        {
            try
            {
                if (payload == null) return;

                var element = ((JsonElement)payload);
                var content = element.GetProperty("content").GetString();
                var filename = element.GetProperty("filename").GetString();

                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var savePath = Path.Combine(documentsPath, "TaoGEST", "Reports");
                Directory.CreateDirectory(savePath);

                var fullPath = Path.Combine(savePath, filename ?? $"DoacGest_Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                File.WriteAllText(fullPath, content ?? string.Empty);

                _logger.LogInformation("Report esportato: {FilePath}", fullPath);

                MessageBox.Show(
                    $"Report esportato in:\n{fullPath}",
                    "Export Completato",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'export del report");
                MessageBox.Show(
                    $"Errore export: {ex.Message}",
                    "Errore",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void PrintReport()
        {
            try
            {
                _logger.LogInformation("Richiesta stampa report");
                DoacGestWebView.CoreWebView2.ExecuteScriptAsync("window.print();");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la stampa");
                MessageBox.Show(
                    $"Errore stampa: {ex.Message}",
                    "Errore",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void ShowNotification(object? payload)
        {
            if (payload == null) return;

            try
            {
                var element = ((JsonElement)payload);
                var message = element.GetProperty("message").GetString();
                var type = element.GetProperty("type").GetString();

                var icon = type switch
                {
                    "success" => MessageBoxImage.Information,
                    "error" => MessageBoxImage.Error,
                    "warning" => MessageBoxImage.Warning,
                    _ => MessageBoxImage.None
                };

                MessageBox.Show(message ?? string.Empty, "DoacGest", MessageBoxButton.OK, icon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la visualizzazione della notifica");
            }
        }

        // ========== COMUNICAZIONE A REACT ==========

        private void SendToReact(string type, object payload)
        {
            try
            {
                var message = new
                {
                    type,
                    payload,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                var json = JsonSerializer.Serialize(message);
                DoacGestWebView.CoreWebView2?.PostWebMessageAsJson(json);
                _logger.LogDebug("Messaggio inviato a React: {Type}", type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'invio messaggio a React");
            }
        }

        private void SendNotification(string message, string type)
        {
            SendToReact("SHOW_NOTIFICATION", new { message, type });
        }

        // ========== MODELLO MESSAGGI ==========

        private class WebMessage
        {
            public string? Type { get; set; }
            public object? Payload { get; set; }
            public long Timestamp { get; set; }
        }
    }
}
