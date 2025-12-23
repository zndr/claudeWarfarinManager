using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WarfarinManager.UI.Services;
using WarfarinManager.UI.ViewModels;
using WarfarinManager.UI.Views.Dashboard;

namespace WarfarinManager.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;

    public MainWindow(MainViewModel viewModel, IServiceProvider serviceProvider, INavigationService navigationService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _navigationService = navigationService;

        // Ascolta i cambi di navigazione
        _navigationService.CurrentViewChanged += OnNavigationChanged;

        // Carica la PatientListView all'avvio
        Loaded += (s, e) =>
        {
            var patientListView = serviceProvider.GetRequiredService<PatientListView>();
            ContentArea.Content = patientListView;
        };

        // Ascolta i cambi di tema per aggiornare l'icona
        ThemeManager.Instance.ThemeChanged += OnThemeChanged;
        UpdateThemeIcon();
    }

    private void OnNavigationChanged(object? sender, System.EventArgs e)
    {
        // Aggiorna il ContentArea quando cambia la navigazione
        ContentArea.Content = _navigationService.CurrentView;
    }

    private void ThemeToggle_Click(object sender, RoutedEventArgs e)
    {
        ThemeManager.Instance.ToggleTheme();
    }

    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        UpdateThemeIcon();
    }

    private void UpdateThemeIcon()
    {
        // Cambia l'icona in base al tema corrente
        var iconResource = ThemeManager.Instance.CurrentTheme == AppTheme.Light
            ? "Icon.Moon"  // Se il tema è chiaro, mostra la luna (per passare al dark)
            : "Icon.Sun";   // Se il tema è scuro, mostra il sole (per passare al light)

        var iconText = (string)FindResource(iconResource);
        ThemeIcon.Text = iconText;
        MenuThemeIcon.Text = iconText;
    }
}
