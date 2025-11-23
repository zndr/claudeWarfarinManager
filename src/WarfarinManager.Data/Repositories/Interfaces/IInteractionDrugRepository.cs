using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Repositories.Interfaces;

/// <summary>
/// Repository per InteractionDrug (lookup table)
/// </summary>
public interface IInteractionDrugRepository : IRepository<InteractionDrug>
{
    /// <summary>
    /// Trova farmaco per nome esatto
    /// </summary>
    Task<InteractionDrug?> FindByNameAsync(string drugName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ricerca farmaci per nome parziale (autocomplete)
    /// </summary>
    Task<List<InteractionDrug>> SearchByNameAsync(string searchTerm, int maxResults, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ottieni farmaci ad alto rischio (OR > 2.0)
    /// </summary>
    Task<List<InteractionDrug>> GetHighRiskDrugsAsync(CancellationToken cancellationToken = default);
}
