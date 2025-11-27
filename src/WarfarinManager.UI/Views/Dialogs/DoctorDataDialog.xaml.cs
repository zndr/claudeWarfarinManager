using System.Windows;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Dialogs;

/// <summary>
/// Interaction logic for DoctorDataDialog.xaml
/// </summary>
public partial class DoctorDataDialog : Window
{
    private readonly DoctorDataViewModel _viewModel;

    public DoctorDataDialog(DoctorDataViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // Carica i dati esistenti
        Loaded += async (s, e) => await viewModel.LoadDataAsync();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
