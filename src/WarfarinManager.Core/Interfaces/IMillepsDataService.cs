using WarfarinManager.Shared.Models;

namespace WarfarinManager.Core.Interfaces;

/// <summary>
/// Servizio per l'estrazione di dati clinici dal database Milleps
/// </summary>
public interface IMillepsDataService
{
    /// <summary>
    /// Estrae i dati biometrici più recenti (peso e altezza) per un paziente
    /// </summary>
    /// <param name="codiceFiscale">Codice fiscale del paziente</param>
    /// <returns>Dati biometrici o null se non trovati</returns>
    Task<MillepsBiometricData?> GetBiometricDataAsync(string codiceFiscale);

    /// <summary>
    /// Estrae gli esami di laboratorio recenti per un paziente
    /// </summary>
    /// <param name="codiceFiscale">Codice fiscale del paziente</param>
    /// <param name="fromDate">Data minima degli esami (default: 30 giorni fa)</param>
    /// <returns>Collezione di esami di laboratorio</returns>
    Task<MillepsLabResultsCollection> GetLabResultsAsync(string codiceFiscale, DateTime? fromDate = null);

    /// <summary>
    /// Estrae le terapie continuative attive per un paziente
    /// </summary>
    /// <param name="codiceFiscale">Codice fiscale del paziente</param>
    /// <returns>Lista delle terapie attive con codice ATC</returns>
    Task<List<MillepsMedication>> GetActiveMedicationsAsync(string codiceFiscale);

    /// <summary>
    /// Testa la connessione al database Milleps
    /// </summary>
    /// <returns>True se la connessione ha successo</returns>
    Task<bool> TestConnectionAsync();

    /// <summary>
    /// Ottiene il codice interno del paziente in Milleps dal codice fiscale
    /// </summary>
    /// <param name="codiceFiscale">Codice fiscale del paziente</param>
    /// <returns>Codice interno Milleps o null se non trovato</returns>
    Task<string?> GetMillepsPatientCodeAsync(string codiceFiscale);

    /// <summary>
    /// Ottiene il codice utente (userid) del medico in Milleps dal codice fiscale
    /// </summary>
    /// <param name="codiceFiscaleMedico">Codice fiscale del medico</param>
    /// <returns>UserId Milleps (pa_medi) o null se non trovato</returns>
    Task<string?> GetMillepsDoctorCodeAsync(string codiceFiscaleMedico);

    /// <summary>
    /// Estrae i dati antropometrici e di laboratorio per un paziente usando i codici Millewin
    /// </summary>
    /// <param name="codiceFiscalePaziente">Codice fiscale del paziente (CFpazi)</param>
    /// <param name="codiceFiscaleMedico">Codice fiscale del medico per ottenere pa_medi</param>
    /// <returns>Dati combinati antropometrici e laboratorio</returns>
    Task<MillepsPatientData?> GetPatientDataAsync(string codiceFiscalePaziente, string codiceFiscaleMedico);

    /// <summary>
    /// Estrae i dati antropometrici e di laboratorio usando il codice Millewin diretto del paziente.
    /// Questo metodo è più efficiente perché evita la ricerca per codice fiscale.
    /// </summary>
    /// <param name="millewinPatientCode">Codice univoco del paziente in Millewin (p.codice)</param>
    /// <param name="millewinDoctorCode">Codice del medico in Millewin (pa_medi)</param>
    /// <returns>Dati combinati antropometrici e laboratorio</returns>
    Task<MillepsPatientData?> GetPatientDataByCodeAsync(string millewinPatientCode, string millewinDoctorCode);

    /// <summary>
    /// Verifica se un paziente esiste ed è associato al medico specificato.
    /// Usa le clausole obbligatorie (convenzione, non deceduto, non revocato).
    /// </summary>
    /// <param name="millewinPatientCode">Codice univoco del paziente in Millewin (p.codice)</param>
    /// <param name="millewinDoctorCode">Codice del medico in Millewin (pa_medi)</param>
    /// <returns>True se il paziente esiste ed è valido per il medico</returns>
    Task<bool> ValidatePatientAsync(string millewinPatientCode, string millewinDoctorCode);
}
