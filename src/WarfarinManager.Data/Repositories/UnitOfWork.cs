using Microsoft.EntityFrameworkCore.Storage;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Repositories.Interfaces;

namespace WarfarinManager.Data.Repositories;

/// <summary>
/// Unit of Work implementation
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly WarfarinDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private IPatientRepository? _patients;
    private IINRControlRepository? _inrControls;
    private IInteractionDrugRepository? _interactionDrugs;
    private IIndicationRepository? _indications;
    private IBridgeTherapyPlanRepository? _bridgeTherapyPlans;
    private IRepository<Entities.AdverseEvent>? _adverseEvents;

    public UnitOfWork(WarfarinDbContext context)
    {
        _context = context;
    }

    public IPatientRepository Patients =>
        _patients ??= new PatientRepository(_context);

    public IINRControlRepository INRControls =>
        _inrControls ??= new INRControlRepository(_context);

    public IInteractionDrugRepository InteractionDrugs =>
        _interactionDrugs ??= new InteractionDrugRepository(_context);

    public IIndicationRepository Indications =>
        _indications ??= new IndicationRepository(_context);

    public IBridgeTherapyPlanRepository BridgeTherapyPlans =>
        _bridgeTherapyPlans ??= new BridgeTherapyPlanRepository(_context);

    public IRepository<Entities.AdverseEvent> AdverseEvents =>
        _adverseEvents ??= new Repository<Entities.AdverseEvent>(_context);

    public WarfarinDbContext Database => _context;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
