using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Configuration;

/// <summary>
/// Configurazione entità AdverseEvent
/// </summary>
public class AdverseEventConfiguration : IEntityTypeConfiguration<AdverseEvent>
{
    public void Configure(EntityTypeBuilder<AdverseEvent> builder)
    {
        builder.ToTable("AdverseEvents");
        
        builder.HasKey(a => a.Id);
        
        // Proprietà
        builder.Property(a => a.EventDate)
            .IsRequired();
            
        builder.Property(a => a.EventType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(a => a.HemorrhagicCategory)
            .HasConversion<string>()
            .HasMaxLength(30);
            
        builder.Property(a => a.ThromboticCategory)
            .HasConversion<string>()
            .HasMaxLength(30);
            
        builder.Property(a => a.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(a => a.INRAtEvent)
            .HasPrecision(4, 2);
            
        builder.Property(a => a.WeeklyDoseAtEvent)
            .HasPrecision(5, 2);
            
        builder.Property(a => a.Management)
            .HasColumnType("TEXT");
            
        builder.Property(a => a.Outcome)
            .HasMaxLength(500);
            
        // Indici
        builder.HasIndex(a => new { a.PatientId, a.EventDate })
            .IsDescending(false, true)
            .HasDatabaseName("IX_AdverseEvents_Patient_Date");
            
        // Relazioni
        builder.HasOne(a => a.Patient)
            .WithMany(p => p.AdverseEvents)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(a => a.LinkedINRControl)
            .WithMany()
            .HasForeignKey(a => a.LinkedINRControlId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
