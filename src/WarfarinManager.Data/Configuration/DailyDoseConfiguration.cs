using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Configuration;

/// <summary>
/// Configurazione entità DailyDose
/// </summary>
public class DailyDoseConfiguration : IEntityTypeConfiguration<DailyDose>
{
    public void Configure(EntityTypeBuilder<DailyDose> builder)
    {
        builder.ToTable("DailyDoses");
        
        builder.HasKey(d => d.Id);
        
        // Proprietà
        builder.Property(d => d.DayOfWeek)
            .IsRequired();
            
        builder.Property(d => d.DoseMg)
            .IsRequired()
            .HasPrecision(4, 2); // es. 5.00, 2.50
            
        // Vincolo: DayOfWeek deve essere tra 1 e 7
        builder.HasCheckConstraint("CK_DailyDoses_DayOfWeek", "[DayOfWeek] BETWEEN 1 AND 7");
        
        // Relazioni
        builder.HasOne(d => d.INRControl)
            .WithMany(i => i.DailyDoses)
            .HasForeignKey(d => d.INRControlId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
