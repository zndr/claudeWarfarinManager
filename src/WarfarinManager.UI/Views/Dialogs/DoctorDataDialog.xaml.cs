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

        // Sottoscrivi l'evento di salvataggio completato
        _viewModel.SaveCompleted += OnSaveCompleted;

        // Carica i dati esistenti
        Loaded += async (s, e) => await viewModel.LoadDataAsync();

        // De-registra l'evento quando la finestra viene chiusa
        Closing += (s, e) => _viewModel.SaveCompleted -= OnSaveCompleted;
    }

    private void OnSaveCompleted(object? sender, EventArgs e)
    {
        // Chiudi automaticamente il dialog dopo il salvataggio
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
