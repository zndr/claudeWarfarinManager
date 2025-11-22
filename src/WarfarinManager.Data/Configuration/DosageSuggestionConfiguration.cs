using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Configuration;

/// <summary>
/// Configurazione entità DosageSuggestion
/// </summary>
public class DosageSuggestionConfiguration : IEntityTypeConfiguration<DosageSuggestion>
{
    public void Configure(EntityTypeBuilder<DosageSuggestion> builder)
    {
        builder.ToTable("DosageSuggestions");
        
        builder.HasKey(d => d.Id);
        
        // Proprietà
        builder.Property(d => d.GuidelineUsed)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(d => d.SuggestedWeeklyDose)
            .IsRequired()
            .HasPrecision(5, 2);
            
        builder.Property(d => d.LoadingDoseAction)
            .HasMaxLength(200);
            
        builder.Property(d => d.PercentageAdjustment)
            .HasPrecision(5, 2);
            
        builder.Property(d => d.NextControlDays)
            .IsRequired();
            
        builder.Property(d => d.RequiresEBPM)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(d => d.RequiresVitaminK)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(d => d.WeeklySchedule)
            .IsRequired()
            .HasColumnType("TEXT");
            
        builder.Property(d => d.ClinicalNotes)
            .HasColumnType("TEXT");
            
        builder.Property(d => d.ExportedText)
            .HasColumnType("TEXT");
            
        // Relazioni
        builder.HasOne(d => d.INRControl)
            .WithMany(i => i.DosageSuggestions)
            .HasForeignKey(d => d.INRControlId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
