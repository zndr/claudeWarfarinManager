using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Repositories.Interfaces;

/// <summary>
/// Interfaccia repository per DoacMonitoringRecord
/// </summary>
public interface IDoacMonitoringRepository : IRepository<DoacMonitoringRecord>
{
    /// <summary>
    /// Ottiene tutte le rilevazioni di un paziente (ordine discendente per data)
    /// </summary>
    Task<IEnumerable<DoacMonitoringRecord>> GetAllByPatientIdAsync(int patientId);

    /// <summary>
    /// Ottiene l'ultima rilevazione di un paziente
    /// </summary>
    Task<DoacMonitoringRecord?> GetUltimaRilevazioneAsync(int patientId);

    /// <summary>
    /// Ottiene rilevazioni filtrate per intervallo di date
    /// </summary>
    Task<IEnumerable<DoacMonitoringRecord>> GetByDateRangeAsync(int patientId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Ottiene controlli scaduti o in scadenza
    /// </summary>
    Task<IEnumerable<DoacMonitoringRecord>> GetControlliScadutiAsync(int giorniAnticipo = 7);

    /// <summary>
    /// Ottiene pazienti con controllo DOAC in scadenza
    /// </summary>
    Task<IEnumerable<DoacMonitoringRecord>> GetProssimiControlliAsync(DateTime dataInizio, DateTime dataFine);

    /// <summary>
    /// Crea una nuova rilevazione con calcoli automatici
    /// </summary>
    Task<DoacMonitoringRecord> CreateWithCalculationsAsync(DoacMonitoringRecord record);
}
