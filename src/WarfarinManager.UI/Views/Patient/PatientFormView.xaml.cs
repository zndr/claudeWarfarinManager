using System.Windows.Controls;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Patient;

/// <summary>
/// Interaction logic for PatientFormView.xaml
/// </summary>
public partial class PatientFormView : UserControl
{
    public PatientFormView(PatientFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
