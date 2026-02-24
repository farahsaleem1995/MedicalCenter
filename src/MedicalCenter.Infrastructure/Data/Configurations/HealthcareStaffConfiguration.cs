using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for HealthcareStaff.
/// </summary>
public class HealthcareStaffConfiguration : IEntityTypeConfiguration<HealthcareStaff>
{
    public void Configure(EntityTypeBuilder<HealthcareStaff> builder)
    {
        builder.ToTable("HealthcareStaff");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(h => h.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(h => h.IsActive)
            .IsRequired();

        builder.Property(h => h.NationalId)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(h => h.OrganizationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Department)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.CreatedAt)
            .IsRequired();

        builder.Property(h => h.UpdatedAt);

        // Explicit one-to-one relationship: HealthcareStaff.Id is both PK and FK to ApplicationUser.Id
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<HealthcareStaff>(h => h.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Global query filter: Only return active healthcare staff (soft delete)
        builder.HasQueryFilter(h => h.IsActive);
    }
}

