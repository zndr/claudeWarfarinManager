using System.Windows;
using Microsoft.Web.WebView2.Core;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Switch;

/// <summary>
/// Interaction logic for SwitchTherapyView.xaml
/// </summary>
public partial class SwitchTherapyView : Window
{
    private readonly SwitchTherapyViewModel _viewModel;

    public SwitchTherapyView(SwitchTherapyViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;

        // Inizializza WebView2
        InitializeAsync();
    }

    /// <summary>
    /// Imposta il paziente corrente per pre-compilare i dati
    /// </summary>
    public void SetPatient(int? patientId)
    {
        _viewModel.SetCurrentPatient(patientId);
    }

    private async void InitializeAsync()
    {
        try
        {
            // Inizializza WebView2
            await webView.EnsureCoreWebView2Async(null);

            // Configura le impostazioni di WebView2
            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            webView.CoreWebView2.Settings.AreDevToolsEnabled = true; // Abilita DevTools per debug (F12)
            webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            webView.CoreWebView2.Settings.IsZoomControlEnabled = true;
            webView.CoreWebView2.Settings.IsScriptEnabled = true; // Abilita JavaScript
            webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true; // Abilita alert/confirm

            // Eventi per la navigazione
            webView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

            // Inizializza il bridge tra JavaScript e C#
            await _viewModel.InitializeWebViewAsync(webView.CoreWebView2);

            // Aggiungi handler per errori JavaScript
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
                window.addEventListener('error', function(e) {
                    console.error('JavaScript Error:', e.message, e.filename, e.lineno);
                });
            ");

            // Log per debugging (ConsoleMessage disponibile solo in versioni più recenti)
            // webView.CoreWebView2.ConsoleMessage += (sender, args) =>
            // {
            //     System.Diagnostics.Debug.WriteLine($"[WebView Console] {args.Level}: {args.Message}");
            // };
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante l'inizializzazione dello strumento Switch:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CoreWebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        StatusText.Text = "Caricamento in corso...";
    }

    private void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (e.IsSuccess)
        {
            StatusText.Text = "Pronto - Compilare il form per generare il protocollo di switch";
        }
        else
        {
            StatusText.Text = "Errore nel caricamento della pagina";
            MessageBox.Show(
                "Si è verificato un errore durante il caricamento della pagina Switch Therapy.",
                "Errore di caricamento",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        webView?.Reload();
        StatusText.Text = "Pagina ricaricata";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
