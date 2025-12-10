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
            .Include(p => p.AdverseEvents.OrderByDescending(e => e.OnsetDate).Take(5))
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

    public async Task<IEnumerable<Patient>> GetPatientsWithActiveIndicationsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Indications.Where(i => i.IsActive))
            .Where(p => p.Indications.Any(i => i.IsActive))
            .OrderBy(p => p.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Patient>> GetPatientsWithRecentINRAsync(int daysThreshold, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.Today.AddDays(-daysThreshold);
        
        return await _dbSet
            .Include(p => p.INRControls.OrderByDescending(c => c.ControlDate).Take(1))
            .Where(p => p.INRControls.Any(c => c.ControlDate >= cutoffDate))
            .OrderBy(p => p.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        // Alias per SearchByNameAsync - manteniamo compatibilità con test
        return await SearchByNameAsync(searchTerm, cancellationToken);
    }

    public async Task SoftDeleteAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var patient = await _dbSet.FindAsync(new object[] { patientId }, cancellationToken);
        if (patient != null)
        {
            patient.IsDeleted = true;
            patient.DeletedAt = DateTime.UtcNow;
            _dbSet.Update(patient);
        }
    }

    public async Task HardDeleteAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var patient = await _dbSet
            .Include(p => p.INRControls)
                .ThenInclude(c => c.DailyDoses)
            .Include(p => p.INRControls)
                .ThenInclude(c => c.DosageSuggestions)
            .Include(p => p.Indications)
            .Include(p => p.Medications)
            .Include(p => p.AdverseEvents)
            .Include(p => p.BridgeTherapyPlans)
            .Include(p => p.PreTaoAssessments)
            .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);

        if (patient != null)
        {
            // EF Core eliminerà automaticamente le entità correlate grazie al cascade delete
            _dbSet.Remove(patient);
        }
    }

    public async Task RestoreAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var patient = await _dbSet
            .IgnoreQueryFilters() // Per trovare anche i pazienti "eliminati"
            .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);

        if (patient != null)
        {
            patient.IsDeleted = false;
            patient.DeletedAt = null;
            _dbSet.Update(patient);
        }
    }

    public async Task<IEnumerable<Patient>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync(cancellationToken);
    }
}
