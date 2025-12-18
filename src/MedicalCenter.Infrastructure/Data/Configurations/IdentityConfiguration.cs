using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Infrastructure.Identity;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Doctors;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Core.Aggregates.Laboratories;
using MedicalCenter.Core.Aggregates.ImagingCenters;
using MedicalCenter.Core.Aggregates.SystemAdmins;

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
        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Roles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Patient)
            .WithOne()
            .HasForeignKey<Patient>(p => p.Id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Doctor)
            .WithOne()
            .HasForeignKey<Doctor>(d => d.Id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.HealthcareStaff)
            .WithOne()
            .HasForeignKey<HealthcareStaff>(h => h.Id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Laboratory)
            .WithOne()
            .HasForeignKey<Laboratory>(l => l.Id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.ImagingCenter)
            .WithOne()
            .HasForeignKey<ImagingCenter>(i => i.Id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.SystemAdmin)
            .WithOne()
            .HasForeignKey<SystemAdmin>(sa => sa.Id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicationRole>()
            .HasMany(r => r.Users)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        modelBuilder.Entity<ApplicationUserRole>()
            .ToTable("AspNetUserRoles")
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        // Table mappings - these are configured after base.OnModelCreating
        // which already sets up Identity entities, so we're just ensuring table names
        modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
        modelBuilder.Entity<ApplicationRole>().ToTable("AspNetRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("AspNetRoleClaims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("AspNetUserTokens");
    }
}

