using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Configuration;

/// <summary>
/// Configurazione entità Indication
/// </summary>
public class IndicationConfiguration : IEntityTypeConfiguration<Indication>
{
    public void Configure(EntityTypeBuilder<Indication> builder)
    {
        builder.ToTable("Indications");
        
        builder.HasKey(i => i.Id);
        
        // Proprietà
        builder.Property(i => i.IndicationTypeCode)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(i => i.TargetINRMin)
            .IsRequired()
            .HasPrecision(3, 1);
            
        builder.Property(i => i.TargetINRMax)
            .IsRequired()
            .HasPrecision(3, 1);
            
        builder.Property(i => i.StartDate)
            .IsRequired();
            
        builder.Property(i => i.ChangeReason)
            .HasMaxLength(500);
            
        // Indici
        builder.HasIndex(i => new { i.PatientId, i.IsActive })
            .HasDatabaseName("IX_Indications_Patient_Active");
            
        // Relazioni
        builder.HasOne(i => i.Patient)
            .WithMany(p => p.Indications)
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(i => i.IndicationType)
            .WithMany(it => it.Indications)
            .HasForeignKey(i => i.IndicationTypeCode)
            .HasPrincipalKey(it => it.Code)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
