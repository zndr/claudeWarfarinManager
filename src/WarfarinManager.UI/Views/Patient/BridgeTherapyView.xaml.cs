using System.ComponentModel;
using System.Windows.Controls;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Patient;

/// <summary>
/// Interaction logic for BridgeTherapyView.xaml
/// </summary>
public partial class BridgeTherapyView : UserControl
{
    public BridgeTherapyView()
    {
        InitializeComponent();
        
        // Sottoscrive al cambio di DataContext per monitorare errori di validazione
        DataContextChanged += OnDataContextChanged;
    }
    
    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        // Rimuovi handler dal vecchio ViewModel
        if (e.OldValue is BridgeTherapyViewModel oldVm)
        {
            oldVm.PropertyChanged -= OnViewModelPropertyChanged;
        }
        
        // Aggiungi handler al nuovo ViewModel
        if (e.NewValue is BridgeTherapyViewModel newVm)
        {
            newVm.PropertyChanged += OnViewModelPropertyChanged;
        }
    }
    
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Quando HasSurgeryTypeError diventa true, sposta il focus sulla ComboBox
        if (e.PropertyName == nameof(BridgeTherapyViewModel.HasSurgeryTypeError))
        {
            if (sender is BridgeTherapyViewModel vm && vm.HasSurgeryTypeError)
            {
                // Dispatch per assicurarsi che l'UI sia aggiornata
                Dispatcher.InvokeAsync(() =>
                {
                    SurgeryTypeComboBox.Focus();
                    SurgeryTypeComboBox.IsDropDownOpen = true;
                });
            }
        }
    }
}
