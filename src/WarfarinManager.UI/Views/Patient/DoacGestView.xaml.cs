using System.Windows.Controls;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Patient
{
    /// <summary>
    /// Interaction logic for DoacGestView.xaml
    /// </summary>
    public partial class DoacGestView : UserControl
    {
        public DoacGestView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Imposta il ViewModel e inizializza con il paziente
        /// </summary>
        public async Task InitializeAsync(DoacGestViewModel viewModel, int patientId)
        {
            DataContext = viewModel;
            await viewModel.InitializeAsync(patientId);
        }
    }
}
