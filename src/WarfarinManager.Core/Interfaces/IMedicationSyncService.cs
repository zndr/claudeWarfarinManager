namespace WarfarinManager.Core.Interfaces;

/// <summary>
/// Risultato della sincronizzazione farmaci con Milleps
/// </summary>
public record MedicationSyncResult
{
    /// <summary>
    /// Numero di nuovi farmaci aggiunti
    /// </summary>
    public int Added { get; init; }

    /// <summary>
    /// Numero di farmaci aggiornati
    /// </summary>
    public int Updated { get; init; }

    /// <summary>
    /// Numero di farmaci disattivati (non piu in Milleps)
    /// </summary>
    public int Deactivated { get; init; }

    /// <summary>
    /// Numero di farmaci con nuove interazioni rilevate
    /// </summary>
    public int NewInteractions { get; init; }

    /// <summary>
    /// Indica se la sincronizzazione ha avuto successo
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Messaggio di errore (se non riuscita)
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Riepilogo testuale delle modifiche
    /// </summary>
    public string Summary => Success
        ? $"Aggiunti: {Added}, Aggiornati: {Updated}, Disattivati: {Deactivated}"
        : $"Errore: {ErrorMessage}";

    /// <summary>
    /// Indica se ci sono state modifiche
    /// </summary>
    public bool HasChanges => Added > 0 || Updated > 0 || Deactivated > 0;

    /// <summary>
    /// Risultato vuoto (nessuna modifica)
    /// </summary>
    public static MedicationSyncResult Empty => new()
    {
        Success = true,
        Added = 0,
        Updated = 0,
        Deactivated = 0,
        NewInteractions = 0
    };

    /// <summary>
    /// Risultato di errore
    /// </summary>
    public static MedicationSyncResult Error(string message) => new()
    {
        Success = false,
        ErrorMessage = message
    };
}

/// <summary>
/// Servizio per la sincronizzazione dei farmaci concomitanti con Milleps.
/// Importa le terapie continuative dal database Milleps e le sincronizza con il database locale.
/// </summary>
public interface IMedicationSyncService
{
    /// <summary>
    /// Sincronizza i farmaci di un paziente con Milleps.
    /// - Aggiunge nuovi farmaci trovati in Milleps
    /// - Aggiorna farmaci esistenti importati da Milleps
    /// - Disattiva farmaci Milleps non piu presenti
    /// - Non modifica farmaci inseriti manualmente
    /// </summary>
    /// <param name="patientId">ID paziente in TaoGEST</param>
    /// <param name="codiceFiscale">Codice fiscale del paziente</param>
    /// <returns>Risultato della sincronizzazione</returns>
    Task<MedicationSyncResult> SyncMedicationsAsync(int patientId, string codiceFiscale);

    /// <summary>
    /// Verifica se la sincronizzazione e disponibile (integrazione Millewin attiva)
    /// </summary>
    bool IsSyncAvailable { get; }
}
