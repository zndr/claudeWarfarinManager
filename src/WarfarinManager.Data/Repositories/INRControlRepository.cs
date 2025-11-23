using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;

namespace WarfarinManager.Data.Repositories;

/// <summary>
/// Repository per INRControl
/// </summary>
public class INRControlRepository : Repository<INRControl>, IINRControlRepository
{
    public INRControlRepository(WarfarinDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<INRControl>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.ControlDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<INRControl>> GetByPatientIdWithDetailsAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.DailyDoses.OrderBy(d => d.DayOfWeek))
            .Include(c => c.DosageSuggestions)
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.ControlDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<INRControl?> GetLatestByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.DailyDoses.OrderBy(d => d.DayOfWeek))
            .Include(c => c.DosageSuggestions)
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.ControlDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<INRControl>> GetByDateRangeAsync(int patientId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.DailyDoses.OrderBy(d => d.DayOfWeek))
            .Where(c => c.PatientId == patientId && 
                       c.ControlDate >= fromDate && 
                       c.ControlDate <= toDate)
            .OrderBy(c => c.ControlDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<INRControl>> GetPatientINRHistoryAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.DailyDoses.OrderBy(d => d.DayOfWeek))
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.ControlDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<INRControl>> GetINRControlsInDateRangeAsync(int patientId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await GetByDateRangeAsync(patientId, startDate, endDate, cancellationToken);
    }

    public async Task<IEnumerable<INRControl>> GetControlsRequiringFollowUpAsync(int daysThreshold, decimal targetMin, decimal targetMax, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.Today.AddDays(-daysThreshold);
        
        return await _dbSet
            .Include(c => c.Patient)
            .Where(c => c.ControlDate >= cutoffDate && 
                       (c.INRValue < targetMin || c.INRValue > targetMax))
            .OrderByDescending(c => c.ControlDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<INRControl?> GetLastINRControlAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await GetLatestByPatientIdAsync(patientId, cancellationToken);
    }
}
