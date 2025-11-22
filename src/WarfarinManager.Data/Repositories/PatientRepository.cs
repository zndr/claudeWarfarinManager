using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;

namespace WarfarinManager.Data.Repositories;

/// <summary>
/// Repository specifico per Patient
/// </summary>
public class PatientRepository : Repository<Patient>, IPatientRepository
{
    public PatientRepository(WarfarinDbContext context) : base(context)
    {
    }

    public async Task<Patient?> GetByFiscalCodeAsync(string fiscalCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.FiscalCode == fiscalCode, cancellationToken);
    }

    public async Task<Patient?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Indications.Where(i => i.IsActive))
                .ThenInclude(i => i.IndicationType)
            .Include(p => p.Medications.Where(m => m.IsActive))
            .Include(p => p.INRControls.OrderByDescending(c => c.ControlDate).Take(10))
            .Include(p => p.AdverseEvents.OrderByDescending(e => e.EventDate).Take(5))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Patient>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        return await _dbSet
            .Where(p => 
                p.FirstName.ToLower().Contains(lowerSearchTerm) ||
                p.LastName.ToLower().Contains(lowerSearchTerm) ||
                p.FiscalCode.ToLower().Contains(lowerSearchTerm))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Patient>> GetSlowMetabolizersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsSlowMetabolizer)
            .OrderBy(p => p.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Patient>> GetWithControlsDueAsync(DateTime beforeDate, CancellationToken cancellationToken = default)
    {
        // Pazienti che non hanno controlli o il cui ultimo controllo è prima della data specificata
        return await _dbSet
            .Where(p => !p.INRControls.Any() || 
                       p.INRControls.OrderByDescending(c => c.ControlDate).First().ControlDate < beforeDate)
            .Include(p => p.Indications.Where(i => i.IsActive))
            .OrderBy(p => p.LastName)
            .ToListAsync(cancellationToken);
    }
}
