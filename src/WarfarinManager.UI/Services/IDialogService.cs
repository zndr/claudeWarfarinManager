using System.Threading.Tasks;
using System.Windows;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.UI.Services;

/// <summary>
/// Scelta dell'utente per la cancellazione di un paziente
/// </summary>
public enum DeletePatientChoice
{
    Cancel,
    SoftDelete,
    HardDelete
}

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

    /// <summary>
    /// Mostra dialog per modificare un record INR
    /// </summary>
    /// <param name="control">Record INR da modificare</param>
    /// <returns>Record modificato o null se annullato</returns>
    Task<INRControl?> ShowINREditDialogAsync(INRControl control);

    /// <summary>
    /// Mostra dialog per valutazione 4D quando INR è significativamente fuori range
    /// </summary>
    /// <returns>Testo della valutazione 4D o null se annullato</returns>
    Task<string?> ShowFourDEvaluationDialogAsync();

    /// <summary>
    /// Mostra dialog per conferma cancellazione paziente con opzioni soft/hard delete
    /// </summary>
    /// <param name="patientName">Nome completo del paziente</param>
    /// <param name="fiscalCode">Codice fiscale del paziente</param>
    /// <returns>Scelta dell'utente</returns>
    DeletePatientChoice ShowDeletePatientDialog(string patientName, string fiscalCode);
}
