using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Configuration;

/// <summary>
/// Configurazione entità Patient
/// </summary>
public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        
        builder.HasKey(p => p.Id);
        
        // Proprietà
        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(p => p.FiscalCode)
            .IsRequired()
            .HasMaxLength(16)
            .IsFixedLength();
            
        builder.Property(p => p.Phone)
            .HasMaxLength(20);
            
        builder.Property(p => p.Email)
            .HasMaxLength(100);
            
        builder.Property(p => p.Address)
            .HasMaxLength(500);

        // Dati biometrici
        builder.Property(p => p.Weight)
            .HasPrecision(5, 2)
            .HasComment("Peso in kg");

        builder.Property(p => p.Height)
            .HasPrecision(5, 2)
            .HasComment("Altezza in cm");

        // Indici
        builder.HasIndex(p => p.FiscalCode)
            .IsUnique()
            .HasDatabaseName("IX_Patients_FiscalCode");
            
        builder.HasIndex(p => new { p.LastName, p.FirstName })
            .HasDatabaseName("IX_Patients_Name");
            
        // Relazioni
        builder.HasMany(p => p.Indications)
            .WithOne(i => i.Patient)
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(p => p.Medications)
            .WithOne(m => m.Patient)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(p => p.INRControls)
            .WithOne(i => i.Patient)
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(p => p.AdverseEvents)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(p => p.BridgeTherapyPlans)
            .WithOne(b => b.Patient)
            .HasForeignKey(b => b.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Ignore computed properties
        builder.Ignore(p => p.Age);
        builder.Ignore(p => p.FullName);
        builder.Ignore(p => p.BMI);
    }
}
