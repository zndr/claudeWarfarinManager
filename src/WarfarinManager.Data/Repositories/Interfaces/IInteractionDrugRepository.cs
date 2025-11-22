using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Repositories.Interfaces;

/// <summary>
/// Repository per InteractionDrug (lookup table)
/// </summary>
public interface IInteractionDrugRepository : IRepository<InteractionDrug>
{
    Task<InteractionDrug?> GetByDrugNameAsync(string drugName, CancellationToken cancellationToken = default);
    Task<IEnumerable<InteractionDrug>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<InteractionDrug>> GetHighRiskDrugsAsync(CancellationToken cancellationToken = default);
}
