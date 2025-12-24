using System;
using System.Windows;
using WarfarinManager.UI.ViewModels.Tools;

namespace WarfarinManager.UI.Views.Tools;

/// <summary>
/// Interaction logic for WeeklyDoseCalculatorDialog.xaml
/// </summary>
public partial class WeeklyDoseCalculatorDialog : Window
{
    public WeeklyDoseCalculatorDialog(WeeklyDoseCalculatorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }
}
