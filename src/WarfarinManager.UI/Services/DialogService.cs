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
}
