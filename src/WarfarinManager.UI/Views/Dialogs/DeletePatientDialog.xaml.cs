using System.Windows;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.Views.Dialogs;

/// <summary>
/// Dialog per la conferma dell'eliminazione di un paziente con opzioni soft/hard delete
/// </summary>
public partial class DeletePatientDialog : Window
{
    public DeletePatientChoice SelectedChoice { get; private set; } = DeletePatientChoice.Cancel;
    public string PatientInfo { get; }

    public DeletePatientDialog(string patientName, string fiscalCode)
    {
        InitializeComponent();
        PatientInfo = $"{patientName} (CF: {fiscalCode})";
        DataContext = this;
    }

    private void SoftDelete_Click(object sender, RoutedEventArgs e)
    {
        SelectedChoice = DeletePatientChoice.SoftDelete;
        DialogResult = true;
        Close();
    }

    private void HardDelete_Click(object sender, RoutedEventArgs e)
    {
        // Conferma aggiuntiva per hard delete
        var result = MessageBox.Show(
            "Sei ASSOLUTAMENTE SICURO di voler eliminare definitivamente questo paziente?\n\n" +
            "Tutti i controlli INR, le terapie, le note e gli altri dati associati saranno PERSI PER SEMPRE.\n\n" +
            "Questa operazione NON può essere annullata!",
            "Conferma eliminazione definitiva",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (result == MessageBoxResult.Yes)
        {
            SelectedChoice = DeletePatientChoice.HardDelete;
            DialogResult = true;
            Close();
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        SelectedChoice = DeletePatientChoice.Cancel;
        DialogResult = false;
        Close();
    }
}
