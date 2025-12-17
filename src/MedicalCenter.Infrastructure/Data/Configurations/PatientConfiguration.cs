using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Infrastructure.Identity;

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

        // Configure BloodType as owned entity
        builder.OwnsOne(p => p.BloodType, bloodType =>
        {
            bloodType.Property(bt => bt.ABO)
                .HasConversion<string>()
                .HasMaxLength(10)
                .HasColumnName("BloodABO");

            bloodType.Property(bt => bt.Rh)
                .HasConversion<string>()
                .HasMaxLength(10)
                .HasColumnName("BloodRh");
        });

        // Configure collections
        builder.HasMany(p => p.Allergies)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.ChronicDiseases)
            .WithOne(cd => cd.Patient)
            .HasForeignKey(cd => cd.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Medications)
            .WithOne(m => m.Patient)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Surgeries)
            .WithOne(s => s.Patient)
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.Email)
            .IsUnique();

        builder.HasIndex(p => p.NationalId)
            .IsUnique();

        // Explicit one-to-one relationship: Patient.Id is both PK and FK to ApplicationUser.Id
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Patient>(p => p.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Global query filter: Only return active patients (soft delete)
        builder.HasQueryFilter(p => p.IsActive);
    }
}

