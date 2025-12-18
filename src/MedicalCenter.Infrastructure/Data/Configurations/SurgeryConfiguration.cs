using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Core.Aggregates.Patients.Entities;

namespace MedicalCenter.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for Surgery entity.
/// </summary>
public class SurgeryConfiguration : IEntityTypeConfiguration<Surgery>
{
    public void Configure(EntityTypeBuilder<Surgery> builder)
    {
        builder.ToTable("Surgeries");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.PatientId)
            .IsRequired();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Date)
            .IsRequired();

        builder.Property(s => s.Surgeon)
            .HasMaxLength(200);

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);

        builder.HasIndex(s => s.PatientId);

        builder.HasQueryFilter(s => s.Patient.IsActive);
    }
}

