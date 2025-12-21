using System.ComponentModel;
using System.Windows;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Patient;

/// <summary>
/// Interaction logic for NewPatientWizardView.xaml
/// </summary>
public partial class NewPatientWizardView : Window
{
    private IndicationFormView? _indicationFormView;
    private WizardPreTaoContraindicationsView? _preTaoContraindicationsView;
    private WizardCha2ds2VascView? _cha2ds2VascView;
    private WizardHasBledView? _hasBledView;

    public NewPatientWizardView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is NewPatientWizardViewModel viewModel)
        {
            // Crea le view con i loro DataContext
            _indicationFormView = new IndicationFormView
            {
                DataContext = viewModel.IndicationFormViewModel
            };

            _preTaoContraindicationsView = new WizardPreTaoContraindicationsView
            {
                DataContext = viewModel.PreTaoAssessmentViewModel
            };

            _cha2ds2VascView = new WizardCha2ds2VascView
            {
                DataContext = viewModel.PreTaoAssessmentViewModel
            };

            _hasBledView = new WizardHasBledView
            {
                DataContext = viewModel.PreTaoAssessmentViewModel
            };

            // Imposta la view iniziale
            StepContentControl.Content = _indicationFormView;

            // Sottoscrivi ai cambiamenti dello step
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not NewPatientWizardViewModel viewModel)
            return;

        if (e.PropertyName == nameof(NewPatientWizardViewModel.CurrentStep))
        {
            StepContentControl.Content = viewModel.CurrentStep switch
            {
                1 => _indicationFormView,
                2 => _preTaoContraindicationsView,
                3 => _cha2ds2VascView,
                4 => _hasBledView,
                _ => null
            };
        }
        else if (e.PropertyName == nameof(NewPatientWizardViewModel.ShouldCloseWizard))
        {
            if (viewModel.ShouldCloseWizard)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
