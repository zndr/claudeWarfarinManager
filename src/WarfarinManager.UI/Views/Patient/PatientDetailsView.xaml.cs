using System.Windows.Controls;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Patient;

/// <summary>
/// Interaction logic for PatientDetailsView.xaml
/// </summary>
public partial class PatientDetailsView : UserControl
{
    public PatientDetailsView(PatientDetailsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
