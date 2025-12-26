using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.Data.Services;

namespace WarfarinManager.Data.Repositories;

/// <summary>
/// Repository per operazioni CRUD su DoacMonitoringRecord
/// </summary>
public class DoacMonitoringRepository : Repository<DoacMonitoringRecord>, IDoacMonitoringRepository
{
    private readonly ITerapiaContinuativaRepository _terapieRepository;

    public DoacMonitoringRepository(
        WarfarinDbContext context,
        ITerapiaContinuativaRepository terapieRepository) : base(context)
    {
        _terapieRepository = terapieRepository;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DoacMonitoringRecord>> GetAllByPatientIdAsync(int patientId)
    {
        return await _context.Set<DoacMonitoringRecord>()
            .Include(d => d.Patient)
            .Where(d => d.PatientId == patientId)
            .OrderByDescending(d => d.DataRilevazione)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<DoacMonitoringRecord?> GetUltimaRilevazioneAsync(int patientId)
    {
        return await _context.Set<DoacMonitoringRecord>()
            .Include(d => d.Patient)
            .Where(d => d.PatientId == patientId)
            .OrderByDescending(d => d.DataRilevazione)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DoacMonitoringRecord>> GetByDateRangeAsync(int patientId, DateTime startDate, DateTime endDate)
    {
        return await _context.Set<DoacMonitoringRecord>()
            .Include(d => d.Patient)
            .Where(d => d.PatientId == patientId &&
                       d.DataRilevazione >= startDate &&
                       d.DataRilevazione <= endDate)
            .OrderByDescending(d => d.DataRilevazione)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DoacMonitoringRecord>> GetControlliScadutiAsync(int giorniAnticipo = 7)
    {
        var dataLimite = DateTime.Today.AddDays(giorniAnticipo);

        return await _context.Set<DoacMonitoringRecord>()
            .Include(d => d.Patient)
            .Where(d => d.DataProssimoControllo.HasValue &&
                       d.DataProssimoControllo.Value <= dataLimite)
            .OrderBy(d => d.DataProssimoControllo)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DoacMonitoringRecord>> GetProssimiControlliAsync(DateTime dataInizio, DateTime dataFine)
    {
        return await _context.Set<DoacMonitoringRecord>()
            .Include(d => d.Patient)
            .Where(d => d.DataProssimoControllo.HasValue &&
                       d.DataProssimoControllo.Value >= dataInizio &&
                       d.DataProssimoControllo.Value <= dataFine)
            .OrderBy(d => d.DataProssimoControllo)
            .ToListAsync();
    }

    /// <summary>
    /// Crea una nuova rilevazione con calcoli automatici
    /// </summary>
    public async Task<DoacMonitoringRecord> CreateWithCalculationsAsync(DoacMonitoringRecord record)
    {
        if (record.Patient == null)
        {
            // Carica paziente se non presente
            record.Patient = await _context.Patients.FindAsync(record.PatientId);
        }

        if (record.Patient != null)
        {
            // Calcola CrCl se abilitato
            if (record.CrCl_Calcolato && record.Peso.HasValue && record.Creatinina.HasValue)
            {
                record.CrCl_Cockroft = DoacCalculationService.CalcolaCockcroftGault(record, record.Patient);
            }

            // Rileva farmaci a rischio
            record.Antiaggreganti = await _terapieRepository.HasAntiaggregantiAttiviAsync(record.PatientId);
            record.FANS = await _terapieRepository.HasFANSAttiviAsync(record.PatientId);

            // Calcola HAS-BLED
            record.HasBledScore = DoacCalculationService.CalcolaHasBledScore(record, record.Patient);

            // Determina dosaggio se DOAC specificato
            if (!string.IsNullOrEmpty(record.DoacSelezionato) && !string.IsNullOrEmpty(record.Indicazione))
            {
                var dosaggio = DoacCalculationService.DeterminaDosaggio(
                    record,
                    record.Patient,
                    record.DoacSelezionato,
                    record.Indicazione
                );

                record.DosaggioSuggerito = dosaggio.Dose;
                record.RazionaleDosaggio = dosaggio.RazionaleCompleto;
            }

            // Calcola data prossimo controllo
            record.CalcolaDataProssimoControllo();
        }

        await AddAsync(record);
        await _context.SaveChangesAsync();

        return record;
    }
}
