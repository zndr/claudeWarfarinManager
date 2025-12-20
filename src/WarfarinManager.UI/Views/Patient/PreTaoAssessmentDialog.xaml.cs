using System.Windows;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Patient;

public partial class PreTaoAssessmentDialog : Window
{
    private readonly PreTaoAssessmentViewModel _viewModel;

    public PreTaoAssessmentDialog(PreTaoAssessmentViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveAsync();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
