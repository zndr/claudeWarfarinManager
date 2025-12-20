namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Livello di urgenza della situazione clinica
/// Determina la tempistica di intervento e follow-up
/// </summary>
public enum UrgencyLevel
{
    /// <summary>
    /// Situazione di routine
    /// - INR in range o lievemente fuori range
    /// - Nessuna complicanza acuta
    /// - Follow-up programmato secondo schedule standard
    /// </summary>
    Routine = 0,

    /// <summary>
    /// Situazione urgente
    /// - INR significativamente fuori range
    /// - Richiede aggiustamento dose e controllo ravvicinato (24-72h)
    /// - Monitoraggio attento ma non emergenza
    /// </summary>
    Urgente = 1,

    /// <summary>
    /// Emergenza medica
    /// - INR estremo (≥8.0) o emorragia attiva
    /// - Richiede intervento immediato
    /// - Valutare ricovero/pronto soccorso
    /// - Controllo INR entro 24h obbligatorio
    /// </summary>
    Emergenza = 2
}
