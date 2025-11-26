using CommunityToolkit.Mvvm.ComponentModel;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel principale per MainWindow
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "TaoGEST";

    [ObservableProperty]
    private object? _currentView;

    public MainViewModel()
    {
        // Inizializzazione base
    }
}
