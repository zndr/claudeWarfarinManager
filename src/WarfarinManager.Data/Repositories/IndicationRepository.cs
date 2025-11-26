using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;

namespace WarfarinManager.Data.Repositories;

/// <summary>
/// Repository per la gestione delle indicazioni terapeutiche
/// </summary>
public class IndicationRepository : Repository<Indication>, IIndicationRepository
{
    public IndicationRepository(WarfarinDbContext context) : base(context)
    {
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<Indication>> GetIndicationsByPatientIdAsync(int patientId)
    {
        return await _context.Set<Indication>()
            .Include(i => i.IndicationType)
            .Where(i => i.PatientId == patientId)
            .OrderByDescending(i => i.IsActive)
            .ThenByDescending(i => i.StartDate)
            .ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<Indication?> GetActiveIndicationAsync(int patientId)
    {
        return await _context.Set<Indication>()
            .Include(i => i.IndicationType)
            .FirstOrDefaultAsync(i => i.PatientId == patientId && i.IsActive);
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<Indication>> GetIndicationsWithTypeAsync(int patientId)
    {
        return await _context.Set<Indication>()
            .Include(i => i.IndicationType)
            .Where(i => i.PatientId == patientId)
            .OrderByDescending(i => i.StartDate)
            .ToListAsync();
    }
}
