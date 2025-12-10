using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Repositories.Interfaces;

/// <summary>
/// Repository specifico per Patient con query custom
/// </summary>
public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetByFiscalCodeAsync(string fiscalCode, CancellationToken cancellationToken = default);
    Task<Patient?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> GetSlowMetabolizersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> GetWithControlsDueAsync(DateTime beforeDate, CancellationToken cancellationToken = default);
    
    // Metodi per integration tests
    Task<IEnumerable<Patient>> GetPatientsWithActiveIndicationsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> GetPatientsWithRecentINRAsync(int daysThreshold, CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm, CancellationToken cancellationToken = default);

    // Metodi per cancellazione
    /// <summary>
    /// Soft delete: marca il paziente come eliminato senza rimuoverlo dal database
    /// </summary>
    Task SoftDeleteAsync(int patientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard delete: elimina definitivamente il paziente e tutti i suoi dati correlati
    /// </summary>
    Task HardDeleteAsync(int patientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ripristina un paziente precedentemente eliminato con soft delete
    /// </summary>
    Task RestoreAsync(int patientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ottiene tutti i pazienti inclusi quelli eliminati (soft delete)
    /// </summary>
    Task<IEnumerable<Patient>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);
}
