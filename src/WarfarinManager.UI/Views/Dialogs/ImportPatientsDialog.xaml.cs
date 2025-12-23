using System.Windows;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Dialogs;

public partial class ImportPatientsDialog : Window
{
    public ImportPatientsDialog(ImportPatientsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
