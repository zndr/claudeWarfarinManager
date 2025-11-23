using System.Windows;

namespace WarfarinManager.UI.Services;

/// <summary>
/// Servizio per gestire dialoghi e messaggi all'utente
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Mostra un messaggio informativo
    /// </summary>
    void ShowInformation(string message, string title = "Informazione");

    /// <summary>
    /// Mostra un messaggio di errore
    /// </summary>
    void ShowError(string message, string title = "Errore");

    /// <summary>
    /// Mostra un messaggio di warning
    /// </summary>
    void ShowWarning(string message, string title = "Attenzione");

    /// <summary>
    /// Mostra una richiesta di conferma (Sì/No)
    /// </summary>
    /// <returns>True se l'utente ha confermato</returns>
    bool ShowConfirmation(string message, string title = "Conferma");

    /// <summary>
    /// Mostra una richiesta con tre opzioni (Sì/No/Annulla)
    /// </summary>
    MessageBoxResult ShowQuestion(string message, string title = "Domanda");
}
