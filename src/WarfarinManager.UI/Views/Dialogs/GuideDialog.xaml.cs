using System.Windows;
using Microsoft.Web.WebView2.Core;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Dialogs;

/// <summary>
/// Interaction logic for GuideDialog.xaml
/// </summary>
public partial class GuideDialog : Window
{
    private readonly GuideViewModel _viewModel;

    public GuideDialog(GuideViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // Inizializza WebView2
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        try
        {
            await webView.EnsureCoreWebView2Async(null);

            // Configura le impostazioni di WebView2
            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            webView.CoreWebView2.Settings.AreDevToolsEnabled = true; // Abilita DevTools per debug
            webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            webView.CoreWebView2.Settings.IsZoomControlEnabled = true;
            webView.CoreWebView2.Settings.IsScriptEnabled = true; // Abilita JavaScript
            webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true; // Abilita alert/confirm

            // Eventi per la navigazione
            webView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

            // Aggiungi handler per errori JavaScript
            webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
                window.addEventListener('error', function(e) {
                    console.error('JavaScript Error:', e.message, e.filename, e.lineno);
                });
            ");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante l'inizializzazione del visualizzatore guide:\n{ex.Message}",
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
        StatusText.Text = e.IsSuccess ? "Guida caricata" : "Errore nel caricamento";
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        webView?.Reload();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
