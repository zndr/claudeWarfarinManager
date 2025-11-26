using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;

namespace WarfarinManager.Data.Repositories;

/// <summary>
/// Repository per la gestione dei piani bridge therapy
/// </summary>
public class BridgeTherapyPlanRepository : Repository<BridgeTherapyPlan>, IBridgeTherapyPlanRepository
{
    public BridgeTherapyPlanRepository(WarfarinDbContext context) : base(context)
    {
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<BridgeTherapyPlan>> GetByPatientIdAsync(int patientId)
    {
        return await _context.Set<BridgeTherapyPlan>()
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.SurgeryDate)
            .ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<BridgeTherapyPlan>> GetFuturePlansAsync(int patientId)
    {
        return await _context.Set<BridgeTherapyPlan>()
            .Where(p => p.PatientId == patientId && p.SurgeryDate >= DateTime.Today)
            .OrderBy(p => p.SurgeryDate)
            .ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<BridgeTherapyPlan?> GetLatestPlanAsync(int patientId)
    {
        return await _context.Set<BridgeTherapyPlan>()
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
