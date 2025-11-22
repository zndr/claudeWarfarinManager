namespace WarfarinManager.Data.Repositories.Interfaces;

/// <summary>
/// Unit of Work pattern per gestire transazioni
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IPatientRepository Patients { get; }
    IINRControlRepository INRControls { get; }
    IInteractionDrugRepository InteractionDrugs { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
