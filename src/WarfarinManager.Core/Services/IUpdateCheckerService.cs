using WarfarinManager.Core.Models;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Servizio per il controllo automatico degli aggiornamenti
/// </summary>
public interface IUpdateCheckerService
{
    /// <summary>
    /// Controlla se è disponibile una nuova versione
    /// </summary>
    /// <param name="currentVersion">Versione attuale dell'applicazione</param>
    /// <param name="cancellationToken">Token di cancellazione</param>
    /// <returns>Informazioni sull'aggiornamento se disponibile, null altrimenti</returns>
    Task<UpdateInfo?> CheckForUpdateAsync(string currentVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se una versione è più recente di un'altra
    /// </summary>
    /// <param name="version">Versione da confrontare</param>
    /// <param name="currentVersion">Versione corrente</param>
    /// <returns>True se version è più recente di currentVersion</returns>
    bool IsNewerVersion(string version, string currentVersion);
}
