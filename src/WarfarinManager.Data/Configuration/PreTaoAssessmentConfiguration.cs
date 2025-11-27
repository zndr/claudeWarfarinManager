using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Configuration;

/// <summary>
/// Configurazione EF Core per PreTaoAssessment
/// </summary>
public class PreTaoAssessmentConfiguration : IEntityTypeConfiguration<PreTaoAssessment>
{
    public void Configure(EntityTypeBuilder<PreTaoAssessment> builder)
    {
        builder.ToTable("PreTaoAssessments");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Proprietà obbligatorie
        builder.Property(p => p.PatientId)
            .IsRequired();

        builder.Property(p => p.AssessmentDate)
            .IsRequired();

        // Indici
        builder.HasIndex(p => p.PatientId);
        builder.HasIndex(p => p.AssessmentDate);

        // Proprietà stringa con lunghezza massima
        builder.Property(p => p.ClinicalNotes)
            .HasMaxLength(2000);

        builder.Property(p => p.Recommendations)
            .HasMaxLength(2000);

        builder.Property(p => p.AssessingPhysician)
            .HasMaxLength(200);

        // Relazioni
        builder.HasOne(p => p.Patient)
            .WithMany(p => p.PreTaoAssessments)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed properties
        builder.Ignore(p => p.CHA2DS2VAScScore);
        builder.Ignore(p => p.HASBLEDScore);
        builder.Ignore(p => p.ThromboticRiskLevel);
        builder.Ignore(p => p.BleedingRiskLevel);
        builder.Ignore(p => p.HasAbsoluteContraindications);
        builder.Ignore(p => p.RelativeContraindicationsCount);
        builder.Ignore(p => p.AdverseEventRiskFactorsCount);
        builder.Ignore(p => p.OverallAssessment);
    }
}
