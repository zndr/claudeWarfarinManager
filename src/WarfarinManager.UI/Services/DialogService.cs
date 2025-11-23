using System.Windows;

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
}
