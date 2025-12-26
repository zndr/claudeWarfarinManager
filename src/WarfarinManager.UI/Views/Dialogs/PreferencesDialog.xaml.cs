using System.Windows;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Dialogs;

/// <summary>
/// Dialog per la gestione delle preferenze applicazione
/// </summary>
public partial class PreferencesDialog : Window
{
    public PreferencesDialog(PreferencesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Owner = Application.Current.MainWindow;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is PreferencesViewModel viewModel)
        {
            await viewModel.SaveCommand.ExecuteAsync(null);
            DialogResult = true;
            Close();
        }
    }
}
