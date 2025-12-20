using System.Windows.Controls;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Patient;

public partial class PatientSummaryView : UserControl
{
    public PatientSummaryView(PatientSummaryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
