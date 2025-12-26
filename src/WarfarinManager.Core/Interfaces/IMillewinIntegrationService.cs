namespace WarfarinManager.Core.Interfaces;

/// <summary>
/// Servizio per la gestione dell'integrazione con Millewin.
/// Controlla lo stato dell'integrazione e la disponibilità della connessione al database milleps.
/// </summary>
public interface IMillewinIntegrationService
{
    /// <summary>
    /// Indica se l'integrazione con Millewin è abilitata nelle preferenze dell'applicazione.
    /// </summary>
    bool IsIntegrationEnabled { get; }

    /// <summary>
    /// Indica se la connessione al database Millewin è disponibile.
    /// Questo valore viene aggiornato quando si testa la connessione.
    /// </summary>
    bool IsConnectionAvailable { get; }

    /// <summary>
    /// Indica se l'integrazione è attiva (abilitata + connessione disponibile).
    /// Questo è il valore da usare per abilitare/disabilitare le funzionalità Millewin.
    /// </summary>
    bool IsIntegrationActive { get; }

    /// <summary>
    /// Abilita o disabilita l'integrazione con Millewin.
    /// </summary>
    /// <param name="enabled">True per abilitare, False per disabilitare.</param>
    void SetIntegrationEnabled(bool enabled);

    /// <summary>
    /// Testa la connessione al database Millewin e aggiorna lo stato di disponibilità.
    /// </summary>
    /// <returns>True se la connessione è riuscita, False altrimenti.</returns>
    Task<bool> TestConnectionAsync();

    /// <summary>
    /// Inizializza il servizio testando la connessione se l'integrazione è abilitata.
    /// Da chiamare all'avvio dell'applicazione.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Evento scatenato quando lo stato dell'integrazione cambia.
    /// </summary>
    event EventHandler<bool>? IntegrationStateChanged;
}
