using System.Windows.Controls;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Dashboard;

/// <summary>
/// Interaction logic for PatientListView.xaml
/// </summary>
public partial class PatientListView : UserControl
{
    public PatientListView(PatientListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Carica i pazienti all'avvio
        Loaded += async (s, e) => await viewModel.LoadPatientsCommand.ExecuteAsync(null);
    }
}
