using System.Windows.Controls;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Patient;

/// <summary>
/// Interaction logic for IndicationFormView.xaml
/// </summary>
public partial class IndicationFormView : UserControl
{
    public IndicationFormView(IndicationFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
