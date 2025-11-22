using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Configuration;

/// <summary>
/// Configurazione entità IndicationType (lookup table)
/// </summary>
public class IndicationTypeConfiguration : IEntityTypeConfiguration<IndicationType>
{
    public void Configure(EntityTypeBuilder<IndicationType> builder)
    {
        builder.ToTable("IndicationTypes");
        
        builder.HasKey(it => it.Id);
        
        // Alternate key per Code
        builder.HasAlternateKey(it => it.Code);
        
        // Proprietà
        builder.Property(it => it.Code)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(it => it.Category)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(it => it.Description)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(it => it.TargetINRMin)
            .IsRequired()
            .HasPrecision(3, 1);
            
        builder.Property(it => it.TargetINRMax)
            .IsRequired()
            .HasPrecision(3, 1);
            
        builder.Property(it => it.TypicalDuration)
            .HasMaxLength(100);
            
        // Indice
        builder.HasIndex(it => it.Code)
            .IsUnique()
            .HasDatabaseName("IX_IndicationTypes_Code");
    }
}
