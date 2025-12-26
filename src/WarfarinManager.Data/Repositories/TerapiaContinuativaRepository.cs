using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;

namespace WarfarinManager.Data.Repositories;

/// <summary>
/// Repository per operazioni CRUD su TerapieContinuative
/// </summary>
public class TerapiaContinuativaRepository : Repository<TerapiaContinuativa>, ITerapiaContinuativaRepository
{
    public TerapiaContinuativaRepository(WarfarinDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TerapiaContinuativa>> GetAllByPatientIdAsync(int patientId, bool soloAttive = false)
    {
        var query = _context.Set<TerapiaContinuativa>()
            .Where(t => t.PatientId == patientId);

        if (soloAttive)
        {
            query = query.Where(t => t.Attiva && (!t.DataFine.HasValue || t.DataFine.Value > DateTime.Now));
        }

        return await query
            .OrderByDescending(t => t.Attiva)
            .ThenBy(t => t.Classe)
            .ThenBy(t => t.PrincipioAttivo)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> HasAntiaggregantiAttiviAsync(int patientId)
    {
        var terapie = await GetAllByPatientIdAsync(patientId, soloAttive: true);
        return terapie.Any(t => t.IsAntiaggregante);
    }

    /// <inheritdoc/>
    public async Task<bool> HasFANSAttiviAsync(int patientId)
    {
        var terapie = await GetAllByPatientIdAsync(patientId, soloAttive: true);
        return terapie.Any(t => t.IsFANS);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TerapiaContinuativa>> GetByClasseAsync(int patientId, string classe, bool soloAttive = true)
    {
        var query = _context.Set<TerapiaContinuativa>()
            .Where(t => t.PatientId == patientId)
            .Where(t => t.Classe.Contains(classe));

        if (soloAttive)
        {
            query = query.Where(t => t.Attiva && (!t.DataFine.HasValue || t.DataFine.Value > DateTime.Now));
        }

        return await query.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TerapiaContinuativa>> GetTerapieAttiveAsync(int patientId)
    {
        return await GetAllByPatientIdAsync(patientId, soloAttive: true);
    }
}
