using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Patient
{
    /// <summary>
    /// Interaction logic for DoacGestWindow.xaml
    /// </summary>
    public partial class DoacGestWindow : Window
    {
        private readonly DoacGestViewModel _viewModel;
        private readonly int? _patientId;

        public DoacGestWindow(IServiceProvider serviceProvider, int? patientId = null)
        {
            InitializeComponent();

            _patientId = patientId;
            _viewModel = serviceProvider.GetRequiredService<DoacGestViewModel>();

            Loaded += OnWindowLoaded;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_patientId.HasValue)
                {
                    await DoacGestViewControl.InitializeAsync(_viewModel, _patientId.Value);
                }
                else
                {
                    // Se non c'è paziente, mostra dialog di selezione paziente
                    MessageBox.Show(
                        "Selezionare un paziente dalla lista pazienti per utilizzare DoacGest.",
                        "Paziente richiesto",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Errore durante il caricamento di DoacGest:\n{ex.Message}",
                    "Errore",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Close();
            }
        }
    }
}
