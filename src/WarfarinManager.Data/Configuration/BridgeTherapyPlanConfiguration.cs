using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Configuration;

/// <summary>
/// Configurazione entità BridgeTherapyPlan
/// </summary>
public class BridgeTherapyPlanConfiguration : IEntityTypeConfiguration<BridgeTherapyPlan>
{
    public void Configure(EntityTypeBuilder<BridgeTherapyPlan> builder)
    {
        builder.ToTable("BridgeTherapyPlans");
        
        builder.HasKey(b => b.Id);
        
        // Proprietà
        builder.Property(b => b.SurgeryDate)
            .IsRequired();
            
        builder.Property(b => b.SurgeryType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);
            
        builder.Property(b => b.ThromboembolicRisk)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(b => b.BridgeRecommended)
            .IsRequired();
            
        builder.Property(b => b.ProtocolText)
            .HasColumnType("TEXT");
            
        // Relazioni
        builder.HasOne(b => b.Patient)
            .WithMany(p => p.BridgeTherapyPlans)
            .HasForeignKey(b => b.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
