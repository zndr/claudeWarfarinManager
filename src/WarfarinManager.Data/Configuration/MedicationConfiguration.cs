using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Configuration;

/// <summary>
/// Configurazione entità Medication
/// </summary>
public class MedicationConfiguration : IEntityTypeConfiguration<Medication>
{
    public void Configure(EntityTypeBuilder<Medication> builder)
    {
        builder.ToTable("Medications");
        
        builder.HasKey(m => m.Id);
        
        // Proprietà
        builder.Property(m => m.MedicationName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(m => m.Dosage)
            .HasMaxLength(100);
            
        builder.Property(m => m.Frequency)
            .HasMaxLength(100);
            
        builder.Property(m => m.StartDate)
            .IsRequired();
            
        builder.Property(m => m.InteractionDetails)
            .HasColumnType("TEXT");
            
        // Indici
        builder.HasIndex(m => new { m.PatientId, m.IsActive })
            .HasDatabaseName("IX_Medications_Patient_Active");
            
        // Relazioni
        builder.HasOne(m => m.Patient)
            .WithMany(p => p.Medications)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
