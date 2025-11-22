using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Context;

/// <summary>
/// Database context principale per WarfarinManager
/// </summary>
public class WarfarinDbContext : DbContext
{
    public WarfarinDbContext(DbContextOptions<WarfarinDbContext> options)
        : base(options)
    {
    }
    
    // DbSets - Entità principali
    
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Indication> Indications => Set<Indication>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<INRControl> INRControls => Set<INRControl>();
    public DbSet<DailyDose> DailyDoses => Set<DailyDose>();
    public DbSet<DosageSuggestion> DosageSuggestions => Set<DosageSuggestion>();
    public DbSet<AdverseEvent> AdverseEvents => Set<AdverseEvent>();
    public DbSet<BridgeTherapyPlan> BridgeTherapyPlans => Set<BridgeTherapyPlan>();
    
    // DbSets - Lookup tables
    
    public DbSet<IndicationType> IndicationTypes => Set<IndicationType>();
    public DbSet<InteractionDrug> InteractionDrugs => Set<InteractionDrug>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Applicazione configurazioni
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarfarinDbContext).Assembly);
        
        // Conversioni enum to string per readability
        ConfigureEnumConversions(modelBuilder);
        
        // Indici per performance
        ConfigureIndexes(modelBuilder);
        
        // Seeding dati lookup
        SeedData(modelBuilder);
    }
    
    private static void ConfigureEnumConversions(ModelBuilder modelBuilder)
    {
        // Convert enums to strings in database for better readability
        modelBuilder.Entity<Patient>()
            .Property(p => p.Gender)
            .HasConversion<string>();
            
        modelBuilder.Entity<INRControl>()
            .Property(i => i.PhaseOfTherapy)
            .HasConversion<string>();
            
        modelBuilder.Entity<DosageSuggestion>()
            .Property(d => d.GuidelineUsed)
            .HasConversion<string>();
            
        modelBuilder.Entity<Medication>()
            .Property(m => m.InteractionLevel)
            .HasConversion<string>();
            
        modelBuilder.Entity<AdverseEvent>()
            .Property(a => a.EventType)
            .HasConversion<string>();
            
        modelBuilder.Entity<AdverseEvent>()
            .Property(a => a.HemorrhagicCategory)
            .HasConversion<string>();
            
        modelBuilder.Entity<AdverseEvent>()
            .Property(a => a.ThromboticCategory)
            .HasConversion<string>();
            
        modelBuilder.Entity<AdverseEvent>()
            .Property(a => a.Severity)
            .HasConversion<string>();
            
        modelBuilder.Entity<BridgeTherapyPlan>()
            .Property(b => b.SurgeryType)
            .HasConversion<string>();
            
        modelBuilder.Entity<BridgeTherapyPlan>()
            .Property(b => b.ThromboembolicRisk)
            .HasConversion<string>();
            
        modelBuilder.Entity<InteractionDrug>()
            .Property(i => i.InteractionEffect)
            .HasConversion<string>();
            
        modelBuilder.Entity<InteractionDrug>()
            .Property(i => i.InteractionLevel)
            .HasConversion<string>();
    }
    
    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Patient indexes
        modelBuilder.Entity<Patient>()
            .HasIndex(p => p.FiscalCode)
            .IsUnique();
            
        modelBuilder.Entity<Patient>()
            .HasIndex(p => new { p.LastName, p.FirstName });
            
        // INRControl indexes
        modelBuilder.Entity<INRControl>()
            .HasIndex(i => new { i.PatientId, i.ControlDate })
            .IsDescending(false, true); // Patient ASC, Date DESC
            
        // Medication indexes
        modelBuilder.Entity<Medication>()
            .HasIndex(m => new { m.PatientId, m.IsActive });
            
        // Indication indexes
        modelBuilder.Entity<Indication>()
            .HasIndex(i => new { i.PatientId, i.IsActive });
            
        // AdverseEvent indexes
        modelBuilder.Entity<AdverseEvent>()
            .HasIndex(a => new { a.PatientId, a.EventDate })
            .IsDescending(false, true);
            
        // IndicationType indexes
        modelBuilder.Entity<IndicationType>()
            .HasIndex(it => it.Code)
            .IsUnique();
            
        // InteractionDrug indexes
        modelBuilder.Entity<InteractionDrug>()
            .HasIndex(id => id.DrugName)
            .IsUnique();
    }
    
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seeding dati lookup completi
        Seeding.InteractionDrugSeeder.Seed(modelBuilder);
        Seeding.IndicationTypeSeeder.Seed(modelBuilder);
    }
    
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
