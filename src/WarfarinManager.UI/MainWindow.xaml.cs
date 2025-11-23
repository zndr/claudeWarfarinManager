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
    }

    private void OnNavigationChanged(object? sender, System.EventArgs e)
    {
        // Aggiorna il ContentArea quando cambia la navigazione
        ContentArea.Content = _navigationService.CurrentView;
    }
}
