using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Data.Repositories;

/// <summary>
/// Repository per InteractionDrug
/// </summary>
public class InteractionDrugRepository : Repository<InteractionDrug>, IInteractionDrugRepository
{
    public InteractionDrugRepository(WarfarinDbContext context) : base(context)
    {
    }

    public async Task<InteractionDrug?> GetByDrugNameAsync(string drugName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(d => d.DrugName.ToLower() == drugName.ToLower(), cancellationToken);
    }

    public async Task<IEnumerable<InteractionDrug>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        return await _dbSet
            .Where(d => d.DrugName.ToLower().Contains(lowerSearchTerm))
            .OrderBy(d => d.DrugName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InteractionDrug>> GetHighRiskDrugsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(d => d.InteractionLevel == InteractionLevel.High)
            .OrderBy(d => d.Category)
            .ThenBy(d => d.DrugName)
            .ToListAsync(cancellationToken);
    }
}
