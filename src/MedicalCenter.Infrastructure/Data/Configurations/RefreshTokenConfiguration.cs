using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for RefreshToken entity.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(e => e.Token);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.ExpiryDate)
            .IsRequired();

        builder.Property(e => e.Revoked)
            .IsRequired()
            .HasDefaultValue(false);
    }
}

