using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Core.Aggregates.Patient;

namespace MedicalCenter.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for ChronicDisease entity.
/// </summary>
public class ChronicDiseaseConfiguration : IEntityTypeConfiguration<ChronicDisease>
{
    public void Configure(EntityTypeBuilder<ChronicDisease> builder)
    {
        builder.ToTable("ChronicDiseases");

        builder.HasKey(cd => cd.Id);

        builder.Property(cd => cd.PatientId)
            .IsRequired();

        builder.Property(cd => cd.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cd => cd.DiagnosisDate)
            .IsRequired();

        builder.Property(cd => cd.Notes)
            .HasMaxLength(1000);

        builder.Property(cd => cd.CreatedAt)
            .IsRequired();

        builder.Property(cd => cd.UpdatedAt);

        builder.HasIndex(cd => cd.PatientId);

        builder.HasQueryFilter(cd => cd.Patient.IsActive);
    }
}

