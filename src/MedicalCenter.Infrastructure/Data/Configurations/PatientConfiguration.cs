using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Core.Entities;

namespace MedicalCenter.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for Patient aggregate.
/// </summary>
public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.NationalId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.DateOfBirth)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);

        // Indexes
        builder.HasIndex(p => p.Email)
            .IsUnique();

        builder.HasIndex(p => p.NationalId)
            .IsUnique();
    }
}

