using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Repositories.Interfaces;

/// <summary>
/// Repository per INRControl con query specifiche
/// </summary>
public interface IINRControlRepository : IRepository<INRControl>
{
    Task<IEnumerable<INRControl>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<INRControl>> GetByPatientIdWithDetailsAsync(int patientId, CancellationToken cancellationToken = default);
    Task<INRControl?> GetLatestByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<INRControl>> GetByDateRangeAsync(int patientId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
}
