using System.Windows.Controls;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Patient;

/// <summary>
/// Interaction logic for IndicationFormView.xaml
/// </summary>
public partial class IndicationFormView : UserControl
{
    public IndicationFormView(IndicationFormViewModel? viewModel = null)
    {
        InitializeComponent();

        // Imposta il DataContext solo se fornito (per DI)
        // Altrimenti sarà impostato dal parent (es. wizard)
        if (viewModel != null)
        {
            DataContext = viewModel;
        }
    }
}
