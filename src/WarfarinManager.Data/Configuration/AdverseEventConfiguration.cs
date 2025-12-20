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
        builder.Property(a => a.OnsetDate)
            .IsRequired();

        builder.Property(a => a.ReactionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.CertaintyLevel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.MeasuresTaken)
            .HasColumnType("TEXT");

        builder.Property(a => a.INRAtEvent)
            .HasPrecision(4, 2);

        builder.Property(a => a.Notes)
            .HasColumnType("TEXT");

        // Indici
        builder.HasIndex(a => new { a.PatientId, a.OnsetDate })
            .IsDescending(false, true)
            .HasDatabaseName("IX_AdverseEvents_Patient_Date");

        builder.HasIndex(a => a.Severity)
            .HasDatabaseName("IX_AdverseEvents_Severity");

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
