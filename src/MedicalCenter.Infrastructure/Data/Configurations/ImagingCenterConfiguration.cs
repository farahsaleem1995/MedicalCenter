using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Core.Aggregates;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for ImagingCenter entity.
/// </summary>
public class ImagingCenterConfiguration : IEntityTypeConfiguration<ImagingCenter>
{
    public void Configure(EntityTypeBuilder<ImagingCenter> builder)
    {
        builder.ToTable("ImagingCenters");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.IsActive)
            .IsRequired();

        builder.Property(i => i.CenterName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.LicenseNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.UpdatedAt);

        // Explicit one-to-one relationship: ImagingCenter.Id is both PK and FK to ApplicationUser.Id
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<ImagingCenter>(i => i.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Global query filter: Only return active imaging centers (soft delete)
        builder.HasQueryFilter(i => i.IsActive);
    }
}
