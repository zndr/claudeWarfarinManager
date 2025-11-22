using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Configuration;

/// <summary>
/// Configurazione entità INRControl
/// </summary>
public class INRControlConfiguration : IEntityTypeConfiguration<INRControl>
{
    public void Configure(EntityTypeBuilder<INRControl> builder)
    {
        builder.ToTable("INRControls");
        
        builder.HasKey(i => i.Id);
        
        // Proprietà
        builder.Property(i => i.ControlDate)
            .IsRequired();
            
        builder.Property(i => i.INRValue)
            .IsRequired()
            .HasPrecision(4, 2);
            
        builder.Property(i => i.CurrentWeeklyDose)
            .IsRequired()
            .HasPrecision(5, 2);
            
        builder.Property(i => i.PhaseOfTherapy)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(i => i.IsCompliant)
            .IsRequired()
            .HasDefaultValue(true);
            
        // Indici
        builder.HasIndex(i => new { i.PatientId, i.ControlDate })
            .IsDescending(false, true)
            .HasDatabaseName("IX_INRControls_Patient_Date");
            
        // Relazioni
        builder.HasOne(i => i.Patient)
            .WithMany(p => p.INRControls)
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(i => i.DailyDoses)
            .WithOne(d => d.INRControl)
            .HasForeignKey(d => d.INRControlId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(i => i.DosageSuggestions)
            .WithOne(d => d.INRControl)
            .HasForeignKey(d => d.INRControlId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
