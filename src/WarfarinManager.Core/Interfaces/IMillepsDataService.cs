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
}
