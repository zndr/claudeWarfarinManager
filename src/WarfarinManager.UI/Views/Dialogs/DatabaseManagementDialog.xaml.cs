using System.Windows;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Dialogs;

/// <summary>
/// Interaction logic for DatabaseManagementDialog.xaml
/// </summary>
public partial class DatabaseManagementDialog : Window
{
    public DatabaseManagementDialog(DatabaseManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Carica statistiche all'avvio
        Loaded += async (s, e) => await viewModel.LoadStatisticsCommand.ExecuteAsync(null);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
