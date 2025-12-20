using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;

namespace WarfarinManager.Data.Repositories;

/// <summary>
/// Implementazione repository InteractionDrug
/// </summary>
public class InteractionDrugRepository : Repository<InteractionDrug>, IInteractionDrugRepository
{
    public InteractionDrugRepository(WarfarinDbContext context) : base(context)
    {
    }

    public async Task<InteractionDrug?> FindByNameAsync(string drugName, CancellationToken cancellationToken = default)
    {
        // Confronto case-insensitive usando ToLower() per SQLite
        var drugNameLower = drugName.ToLower();
        return await _context.InteractionDrugs
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DrugName.ToLower() == drugNameLower, cancellationToken);
    }

    public async Task<List<InteractionDrug>> SearchByNameAsync(
        string searchTerm, 
        int maxResults, 
        CancellationToken cancellationToken = default)
    {
        // Confronto case-insensitive usando ToLower() per SQLite
        var searchTermLower = searchTerm.ToLower();
        return await _context.InteractionDrugs
            .AsNoTracking()
            .Where(d => d.DrugName.ToLower().Contains(searchTermLower))
            .OrderBy(d => d.DrugName)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<InteractionDrug>> GetHighRiskDrugsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.InteractionDrugs
            .AsNoTracking()
            .Where(d => d.OddsRatio >= 2.0m)
            .OrderByDescending(d => d.OddsRatio)
            .ToListAsync(cancellationToken);
    }
}
