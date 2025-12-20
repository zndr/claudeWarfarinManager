using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Configuration;

/// <summary>
/// Configurazione EF Core per DoctorData
/// </summary>
public class DoctorDataConfiguration : IEntityTypeConfiguration<DoctorData>
{
    public void Configure(EntityTypeBuilder<DoctorData> builder)
    {
        builder.ToTable("DoctorData");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Street)
            .HasMaxLength(200);

        builder.Property(d => d.PostalCode)
            .HasMaxLength(10);

        builder.Property(d => d.City)
            .HasMaxLength(100);

        builder.Property(d => d.Phone)
            .HasMaxLength(20);

        builder.Property(d => d.Email)
            .HasMaxLength(100);
    }
}
