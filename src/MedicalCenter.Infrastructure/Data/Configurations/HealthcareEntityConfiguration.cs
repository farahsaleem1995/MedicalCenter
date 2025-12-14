using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Core.Entities;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for HealthcareEntity.
/// </summary>
public class HealthcareEntityConfiguration : IEntityTypeConfiguration<HealthcareEntity>
{
    public void Configure(EntityTypeBuilder<HealthcareEntity> builder)
    {
        builder.ToTable("HealthcareEntities");

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

        builder.Property(h => h.OrganizationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Department)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.CreatedAt)
            .IsRequired();

        builder.Property(h => h.UpdatedAt);

        // Explicit one-to-one relationship: HealthcareEntity.Id is both PK and FK to ApplicationUser.Id
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<HealthcareEntity>(h => h.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Global query filter: Only return active healthcare entities (soft delete)
        builder.HasQueryFilter(h => h.IsActive);
    }
}
