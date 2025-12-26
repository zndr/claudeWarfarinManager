using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Repositories.Interfaces;

/// <summary>
/// Interfaccia repository per TerapiaContinuativa
/// </summary>
public interface ITerapiaContinuativaRepository : IRepository<TerapiaContinuativa>
{
    /// <summary>
    /// Ottiene tutte le terapie di un paziente
    /// </summary>
    Task<IEnumerable<TerapiaContinuativa>> GetAllByPatientIdAsync(int patientId, bool soloAttive = false);

    /// <summary>
    /// Verifica se il paziente ha antiaggreganti attivi
    /// </summary>
    Task<bool> HasAntiaggregantiAttiviAsync(int patientId);

    /// <summary>
    /// Verifica se il paziente ha FANS attivi
    /// </summary>
    Task<bool> HasFANSAttiviAsync(int patientId);

    /// <summary>
    /// Cerca terapie per classe farmacologica
    /// </summary>
    Task<IEnumerable<TerapiaContinuativa>> GetByClasseAsync(int patientId, string classe, bool soloAttive = true);

    /// <summary>
    /// Ottiene tutte le terapie attive di un paziente
    /// </summary>
    Task<IEnumerable<TerapiaContinuativa>> GetTerapieAttiveAsync(int patientId);
}
