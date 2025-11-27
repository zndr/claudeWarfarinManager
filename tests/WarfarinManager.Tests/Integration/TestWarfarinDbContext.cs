using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;

namespace WarfarinManager.Tests.Integration;

/// <summary>
/// DbContext per test che sovrascrive il seeding automatico
/// Evita problemi con HasData e FK constraints nei test
/// </summary>
public class TestWarfarinDbContext : WarfarinDbContext
{
    public TestWarfarinDbContext(DbContextOptions options)
        : base((DbContextOptions<WarfarinDbContext>)options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Chiama il base per avere tutte le configurazioni (schema, relazioni, indici, enum conversions)
        // MA non chiamiamo SeedData
        
        // Applicazione configurazioni
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarfarinDbContext).Assembly);
        
        // Conversioni enum to string
        ConfigureEnumConversions(modelBuilder);
        
        // Indici per performance
        ConfigureIndexes(modelBuilder);
        
        // NON chiamiamo SeedData per evitare problemi con HasData nei test
        // Il seeding sarà fatto manualmente nel TestDbContextFactory
    }
    
    // I metodi ConfigureEnumConversions e ConfigureIndexes sono protected nel base
    // Dobbiamo duplicarli qui (purtroppo)
    private static void ConfigureEnumConversions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Data.Entities.Patient>()
            .Property(p => p.Gender)
            .HasConversion<string>();
            
        modelBuilder.Entity<Data.Entities.INRControl>()
            .Property(i => i.PhaseOfTherapy)
            .HasConversion<string>();
            
        modelBuilder.Entity<Data.Entities.DosageSuggestion>()
            .Property(d => d.GuidelineUsed)
            .HasConversion<string>();
            
        modelBuilder.Entity<Data.Entities.Medication>()
            .Property(m => m.InteractionLevel)
            .HasConversion<string>();
            
        modelBuilder.Entity<Data.Entities.AdverseEvent>()
            .Property(a => a.ReactionType)
            .HasConversion<string>();

        modelBuilder.Entity<Data.Entities.AdverseEvent>()
            .Property(a => a.Severity)
            .HasConversion<string>();

        modelBuilder.Entity<Data.Entities.AdverseEvent>()
            .Property(a => a.CertaintyLevel)
            .HasConversion<string>();
            
        modelBuilder.Entity<Data.Entities.BridgeTherapyPlan>()
            .Property(b => b.SurgeryType)
            .HasConversion<string>();
            
        modelBuilder.Entity<Data.Entities.BridgeTherapyPlan>()
            .Property(b => b.ThromboembolicRisk)
            .HasConversion<string>();
            
        modelBuilder.Entity<Data.Entities.InteractionDrug>()
            .Property(i => i.InteractionEffect)
            .HasConversion<string>();
            
        modelBuilder.Entity<Data.Entities.InteractionDrug>()
            .Property(i => i.InteractionLevel)
            .HasConversion<string>();
    }
    
    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Data.Entities.Patient>()
            .HasIndex(p => p.FiscalCode)
            .IsUnique();
            
        modelBuilder.Entity<Data.Entities.Patient>()
            .HasIndex(p => new { p.LastName, p.FirstName });
            
        modelBuilder.Entity<Data.Entities.INRControl>()
            .HasIndex(i => new { i.PatientId, i.ControlDate })
            .IsDescending(false, true);
            
        modelBuilder.Entity<Data.Entities.Medication>()
            .HasIndex(m => new { m.PatientId, m.IsActive });
            
        modelBuilder.Entity<Data.Entities.Indication>()
            .HasIndex(i => new { i.PatientId, i.IsActive });
            
        modelBuilder.Entity<Data.Entities.AdverseEvent>()
            .HasIndex(a => new { a.PatientId, a.OnsetDate })
            .IsDescending(false, true);
            
        modelBuilder.Entity<Data.Entities.IndicationType>()
            .HasIndex(it => it.Code)
            .IsUnique();
            
        modelBuilder.Entity<Data.Entities.InteractionDrug>()
            .HasIndex(id => id.DrugName)
            .IsUnique();
    }
}
