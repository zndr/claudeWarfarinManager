using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Repositories.Interfaces;

/// <summary>
/// Repository per la gestione delle indicazioni terapeutiche
/// </summary>
public interface IIndicationRepository : IRepository<Indication>
{
    /// <summary>
    /// Ottiene tutte le indicazioni di un paziente
    /// </summary>
    Task<IEnumerable<Indication>> GetIndicationsByPatientIdAsync(int patientId);
    
    /// <summary>
    /// Ottiene l'indicazione attiva di un paziente
    /// </summary>
    Task<Indication?> GetActiveIndicationAsync(int patientId);
    
    /// <summary>
    /// Ottiene le indicazioni con il tipo incluso
    /// </summary>
    Task<IEnumerable<Indication>> GetIndicationsWithTypeAsync(int patientId);
}
