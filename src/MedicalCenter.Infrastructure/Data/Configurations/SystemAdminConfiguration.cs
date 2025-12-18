using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Core.Aggregates.SystemAdmins;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for SystemAdmin entity.
/// </summary>
public class SystemAdminConfiguration : IEntityTypeConfiguration<SystemAdmin>
{
    public void Configure(EntityTypeBuilder<SystemAdmin> builder)
    {
        builder.ToTable("SystemAdmins");

        // Primary key is shared with ApplicationUser (one-to-one)
        builder.HasKey(sa => sa.Id);

        // Configure properties
        builder.Property(sa => sa.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sa => sa.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(sa => sa.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sa => sa.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Enhanced organizational properties
        builder.Property(sa => sa.CorporateId)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(sa => sa.Department)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sa => sa.CreatedAt)
            .IsRequired();

        builder.Property(sa => sa.UpdatedAt);

        // Explicit one-to-one relationship: SystemAdmin.Id is both PK and FK to ApplicationUser.Id
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<SystemAdmin>(sa => sa.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Global query filter for soft delete
        builder.HasQueryFilter(sa => sa.IsActive);

        // Indexes
        builder.HasIndex(sa => sa.Email)
            .IsUnique();

        builder.HasIndex(sa => sa.CorporateId)
            .IsUnique();
            
        builder.HasIndex(sa => sa.Department);

        builder.HasIndex(sa => new { sa.Id, sa.IsActive });
    }
}

