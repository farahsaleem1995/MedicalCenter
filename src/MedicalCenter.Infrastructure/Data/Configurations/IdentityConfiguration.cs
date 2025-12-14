using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for ASP.NET Core Identity tables.
/// Configures table names for all Identity entities.
/// </summary>
public class IdentityConfiguration
{
    /// <summary>
    /// Configures all Identity table names.
    /// This is called from the DbContext OnModelCreating method.
    /// </summary>
    public static void ConfigureIdentityTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("AspNetRoles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("AspNetUserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("AspNetRoleClaims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("AspNetUserTokens");
    }
}

