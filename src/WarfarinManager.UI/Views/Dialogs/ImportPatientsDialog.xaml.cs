using System.Windows;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Dialogs;

public partial class ImportPatientsDialog : Window
{
    private readonly ImportPatientsViewModel _viewModel;

    public ImportPatientsDialog(ImportPatientsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // Esegue inizializzazione automatica quando la finestra è caricata
        Loaded += async (s, e) => await _viewModel.InitializeAsync();
    }
}
