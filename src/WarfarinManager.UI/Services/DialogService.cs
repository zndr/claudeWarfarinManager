using System.Threading.Tasks;
using System.Windows;
using WarfarinManager.Data.Entities;
using WarfarinManager.UI.Views.Dialogs;

#pragma warning disable CS1998 // Async method lacks 'await' operators

namespace WarfarinManager.UI.Services;

/// <summary>
/// Implementazione del servizio di dialoghi usando MessageBox WPF
/// </summary>
public class DialogService : IDialogService
{
    public void ShowInformation(string message, string title = "Informazione")
    {
        MessageBox.Show(
            message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    public void ShowError(string message, string title = "Errore")
    {
        MessageBox.Show(
            message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    public void ShowWarning(string message, string title = "Attenzione")
    {
        MessageBox.Show(
            message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    public bool ShowConfirmation(string message, string title = "Conferma")
    {
        var result = MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        return result == MessageBoxResult.Yes;
    }

    public MessageBoxResult ShowQuestion(string message, string title = "Domanda")
    {
        return MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);
    }

    public async Task<INRControl?> ShowINREditDialogAsync(INRControl control)
    {
        var dialog = new INREditDialog(control);
        var result = dialog.ShowDialog();

        if (result == true)
        {
            return dialog.EditedControl;
        }

        return null;
    }

    public async Task<string?> ShowFourDEvaluationDialogAsync()
    {
        var dialog = new FourDEvaluationDialog();
        var result = dialog.ShowDialog();

        if (result == true)
        {
            return dialog.EvaluationText;
        }

        return null;
    }

    public DeletePatientChoice ShowDeletePatientDialog(string patientName, string fiscalCode)
    {
        var dialog = new DeletePatientDialog(patientName, fiscalCode);
        var result = dialog.ShowDialog();

        if (result != true)
            return DeletePatientChoice.Cancel;

        return dialog.SelectedChoice;
    }

    public bool? ShowNaivePatientDialog(string patientName)
    {
        var result = MessageBox.Show(
            $"Il paziente {patientName} è \"naive\" (inizia con te la fase di induzione della terapia con warfarin)?\n\n" +
            "• Seleziona SÌ se è la prima volta che assume warfarin\n" +
            "• Seleziona NO se l'induzione è già avvenuta (es. durante ricovero ospedaliero)",
            "Paziente Naive - Fase di Induzione",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Cancel)
            return null;

        return result == MessageBoxResult.Yes;
    }

    public void ShowInductionPhaseInfo()
    {
        MessageBox.Show(
            "FASE DI INDUZIONE TERAPIA CON WARFARIN\n\n" +
            "Protocollo raccomandato secondo il Nomogramma di Pengo (2001):\n\n" +
            "1. Somministrare 5 mg di warfarin al giorno per 4 giorni consecutivi\n\n" +
            "2. Al 5° giorno, valutare l'INR e utilizzare il nomogramma di Pengo per stimare il fabbisogno settimanale\n\n" +
            "3. Distribuire il fabbisogno settimanale nei 7 giorni, preferibilmente evitando quarti di compressa\n\n" +
            "NOTA: Il nomogramma sarà disponibile automaticamente al primo inserimento INR.",
            "Fase di Induzione - Nomogramma di Pengo",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    public bool ShowPengoNomogramConfirmation(decimal inrValue)
    {
        var result = MessageBox.Show(
            $"È stato rilevato un valore INR di {inrValue:F1}.\n\n" +
            "Questo controllo corrisponde alla valutazione dopo 4 dosi da 5 mg?\n\n" +
            "Vuoi utilizzare il NOMOGRAMMA DI PENGO (2001) per stimare il fabbisogno settimanale di warfarin?",
            "Conferma Uso Nomogramma di Pengo",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        return result == MessageBoxResult.Yes;
    }

    public void ShowPengoNomogramHtml(decimal? inrValue = null)
    {
        try
        {
            // Il file è in Resources/Guides dopo la sincronizzazione
            var basePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources", "Guides", "nomogramma-pengo.html");

            if (!System.IO.File.Exists(basePath))
            {
                ShowError($"File nomogramma non trovato in:\n{basePath}\n\nVerifica che il file sia stato sincronizzato correttamente.", "File Non Trovato");
                return;
            }

            var url = inrValue.HasValue
                ? $"file:///{basePath.Replace("\\", "/")}?inr={inrValue.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
                : $"file:///{basePath.Replace("\\", "/")}";

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
        catch (System.Exception ex)
        {
            ShowError($"Impossibile aprire il nomogramma di Pengo:\n{ex.Message}", "Errore");
        }
    }
}
