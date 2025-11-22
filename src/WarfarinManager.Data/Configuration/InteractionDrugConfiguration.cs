using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Configuration;

/// <summary>
/// Configurazione entità InteractionDrug (lookup table)
/// </summary>
public class InteractionDrugConfiguration : IEntityTypeConfiguration<InteractionDrug>
{
    public void Configure(EntityTypeBuilder<InteractionDrug> builder)
    {
        builder.ToTable("InteractionDrugs");
        
        builder.HasKey(id => id.Id);
        
        // Proprietà
        builder.Property(id => id.DrugName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(id => id.Category)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(id => id.InteractionEffect)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(id => id.OddsRatio)
            .HasPrecision(5, 2);
            
        builder.Property(id => id.Mechanism)
            .HasMaxLength(500);
            
        builder.Property(id => id.FCSAManagement)
            .HasMaxLength(1000);
            
        builder.Property(id => id.ACCPManagement)
            .HasMaxLength(1000);
            
        builder.Property(id => id.InteractionLevel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
            
        // Indice
        builder.HasIndex(id => id.DrugName)
            .IsUnique()
            .HasDatabaseName("IX_InteractionDrugs_DrugName");
    }
}
