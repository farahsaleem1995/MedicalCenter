using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Core.Aggregates;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for Laboratory entity.
/// </summary>
public class LaboratoryConfiguration : IEntityTypeConfiguration<Laboratory>
{
    public void Configure(EntityTypeBuilder<Laboratory> builder)
    {
        builder.ToTable("Laboratories");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(l => l.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.IsActive)
            .IsRequired();

        builder.Property(l => l.LabName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.LicenseNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.UpdatedAt);

        // Explicit one-to-one relationship: Laboratory.Id is both PK and FK to ApplicationUser.Id
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Laboratory>(l => l.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Global query filter: Only return active laboratories (soft delete)
        builder.HasQueryFilter(l => l.IsActive);
    }
}
