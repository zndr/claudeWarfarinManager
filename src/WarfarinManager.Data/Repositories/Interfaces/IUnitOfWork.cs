using WarfarinManager.Data.Context;

namespace WarfarinManager.Data.Repositories.Interfaces;

/// <summary>
/// Unit of Work pattern per gestire transazioni
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IPatientRepository Patients { get; }
    IINRControlRepository INRControls { get; }
    IInteractionDrugRepository InteractionDrugs { get; }
    IIndicationRepository Indications { get; }
    IBridgeTherapyPlanRepository BridgeTherapyPlans { get; }
    IRepository<Entities.AdverseEvent> AdverseEvents { get; }

    /// <summary>
    /// Accesso diretto al DbContext per query complesse
    /// </summary>
    WarfarinDbContext Database { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
