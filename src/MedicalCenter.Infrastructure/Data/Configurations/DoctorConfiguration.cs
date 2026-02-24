using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Core.Aggregates.Doctors;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for Doctor entity.
/// </summary>
public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(d => d.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.IsActive)
            .IsRequired();

        builder.Property(d => d.NationalId)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(d => d.LicenseNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Specialty)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdatedAt);

        // Explicit one-to-one relationship: Doctor.Id is both PK and FK to ApplicationUser.Id
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Doctor>(d => d.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Global query filter: Only return active doctors (soft delete)
        builder.HasQueryFilter(d => d.IsActive);
    }
}
