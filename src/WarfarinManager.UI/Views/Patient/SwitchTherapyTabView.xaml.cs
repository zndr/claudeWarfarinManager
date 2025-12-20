using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Patient;

/// <summary>
/// UserControl per il tab Switch Terapia nel dettaglio paziente
/// </summary>
public partial class SwitchTherapyTabView : UserControl
{
    private SwitchTherapyViewModel? _viewModel;

    public SwitchTherapyTabView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        try
        {
            // Aspetta che WebView2 sia pronto
            await webView.EnsureCoreWebView2Async(null);

            // Registra l'evento NavigationCompleted
            webView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;

            // Inizializza il ViewModel se presente
            if (DataContext is SwitchTherapyViewModel viewModel)
            {
                _viewModel = viewModel;
                await viewModel.InitializeWebViewAsync(webView.CoreWebView2);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing WebView2: {ex.Message}");
        }
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        // Quando la navigazione è completata, forza la pre-compilazione dei dati
        if (e.IsSuccess && _viewModel != null)
        {
            System.Diagnostics.Debug.WriteLine("Navigation completed, triggering patient data pre-fill");
            _viewModel.RefreshPatientData();
        }
    }
}
