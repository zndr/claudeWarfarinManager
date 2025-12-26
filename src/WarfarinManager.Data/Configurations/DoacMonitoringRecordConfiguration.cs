using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Configurations;

/// <summary>
/// Configurazione Entity Framework per DoacMonitoringRecord
/// </summary>
public class DoacMonitoringRecordConfiguration : IEntityTypeConfiguration<DoacMonitoringRecord>
{
    public void Configure(EntityTypeBuilder<DoacMonitoringRecord> builder)
    {
        builder.ToTable("DoacMonitoring");

        builder.HasKey(e => e.Id);

        // Relazione con Patient
        builder.HasOne(e => e.Patient)
            .WithMany(p => p.DoacMonitoringRecords)
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configurazione decimali
        builder.Property(e => e.Creatinina)
            .HasColumnType("decimal(3,1)");

        builder.Property(e => e.Peso)
            .HasColumnType("decimal(5,1)");

        builder.Property(e => e.Emoglobina)
            .HasColumnType("decimal(4,1)");

        builder.Property(e => e.Ematocrito)
            .HasColumnType("decimal(4,1)");

        builder.Property(e => e.Bilirubina)
            .HasColumnType("decimal(4,1)");

        builder.Property(e => e.GGT)
            .HasColumnType("decimal(5,1)");

        // Default values
        builder.Property(e => e.DataRilevazione)
            .HasDefaultValueSql("GETDATE()");

        builder.Property(e => e.CrCl_Calcolato)
            .HasDefaultValue(true);

        // String lengths
        builder.Property(e => e.DoacSelezionato)
            .HasMaxLength(50);

        builder.Property(e => e.Indicazione)
            .HasMaxLength(200);

        builder.Property(e => e.DosaggioSuggerito)
            .HasMaxLength(100);

        builder.Property(e => e.RazionaleDosaggio)
            .HasMaxLength(1000);
    }
}
