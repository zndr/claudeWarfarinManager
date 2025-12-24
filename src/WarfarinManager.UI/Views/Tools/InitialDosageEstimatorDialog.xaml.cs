using System;
using System.Windows;
using WarfarinManager.UI.ViewModels.Tools;

namespace WarfarinManager.UI.Views.Tools;

/// <summary>
/// Interaction logic for InitialDosageEstimatorDialog.xaml
/// </summary>
public partial class InitialDosageEstimatorDialog : Window
{
    public InitialDosageEstimatorDialog(InitialDosageEstimatorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }
}
