using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
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

        // Gestisce il focus sul campo di ricerca
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PatientListViewModel.ShouldFocusSearchBox))
        {
            var viewModel = DataContext as PatientListViewModel;
            if (viewModel?.ShouldFocusSearchBox == true)
            {
                SearchTextBox.Focus();
                SearchTextBox.SelectAll();
            }
        }
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // Verifica che il doppio click sia su una riga e non sull'header o altro
        if (sender is DataGrid dataGrid && dataGrid.SelectedItem != null)
        {
            var viewModel = DataContext as PatientListViewModel;
            if (viewModel?.OpenPatientDetailsCommand?.CanExecute(null) == true)
            {
                viewModel.OpenPatientDetailsCommand.Execute(null);
            }
        }
    }
}
