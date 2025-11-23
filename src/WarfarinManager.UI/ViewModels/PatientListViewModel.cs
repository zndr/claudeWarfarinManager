using CommunityToolkit.Mvvm.ComponentModel;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per la lista pazienti (placeholder per ora)
/// </summary>
public partial class PatientListViewModel : ObservableObject
{
    [ObservableProperty]
    private string _welcomeMessage = "Lista Pazienti - In sviluppo";

    public PatientListViewModel()
    {
        // Inizializzazione base
    }
}
