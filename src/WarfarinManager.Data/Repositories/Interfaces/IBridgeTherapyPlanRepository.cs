using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Repositories.Interfaces;

/// <summary>
/// Repository per la gestione dei piani bridge therapy
/// </summary>
public interface IBridgeTherapyPlanRepository : IRepository<BridgeTherapyPlan>
{
    /// <summary>
    /// Ottiene tutti i piani bridge di un paziente
    /// </summary>
    Task<IEnumerable<BridgeTherapyPlan>> GetByPatientIdAsync(int patientId);
    
    /// <summary>
    /// Ottiene i piani bridge futuri di un paziente
    /// </summary>
    Task<IEnumerable<BridgeTherapyPlan>> GetFuturePlansAsync(int patientId);
    
    /// <summary>
    /// Ottiene l'ultimo piano bridge di un paziente
    /// </summary>
    Task<BridgeTherapyPlan?> GetLatestPlanAsync(int patientId);
}
