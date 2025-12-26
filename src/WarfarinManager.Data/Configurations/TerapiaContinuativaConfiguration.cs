using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Configurations;

/// <summary>
/// Configurazione Entity Framework per TerapiaContinuativa
/// </summary>
public class TerapiaContinuativaConfiguration : IEntityTypeConfiguration<TerapiaContinuativa>
{
    public void Configure(EntityTypeBuilder<TerapiaContinuativa> builder)
    {
        builder.ToTable("TerapieContinuative");

        builder.HasKey(e => e.Id);

        // Relazione con Patient
        builder.HasOne(e => e.Patient)
            .WithMany(p => p.TerapieContinuative)
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // String lengths
        builder.Property(e => e.Classe)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.PrincipioAttivo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.NomeCommerciale)
            .HasMaxLength(200);

        builder.Property(e => e.Dosaggio)
            .HasMaxLength(100);

        builder.Property(e => e.FrequenzaGiornaliera)
            .HasMaxLength(50);

        builder.Property(e => e.ViaAssunzione)
            .HasMaxLength(50);

        builder.Property(e => e.Indicazione)
            .HasMaxLength(500);

        builder.Property(e => e.Note)
            .HasMaxLength(1000);

        builder.Property(e => e.MotivoSospensione)
            .HasMaxLength(500);

        builder.Property(e => e.FonteDati)
            .HasMaxLength(50);

        // Default values
        builder.Property(e => e.Attiva)
            .HasDefaultValue(true);

        builder.Property(e => e.Importato)
            .HasDefaultValue(false);
    }
}
