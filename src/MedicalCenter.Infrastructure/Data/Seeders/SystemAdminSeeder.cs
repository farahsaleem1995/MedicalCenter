using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedicalCenter.Core.Enums;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Seeders;

/// <summary>
/// Seeds the system administrator user into the database using EF Core HasData.
/// </summary>
public static class SystemAdminSeeder
{
    private const string AdminEmail = "sys.admin@medicalcenter.com";
    private const string AdminPassword = "Admin@123!ChangeMe";
    
    // Pre-computed password hash for Admin@123!ChangeMe using ASP.NET Core Identity's PasswordHasher
    // This must be a constant to ensure deterministic migrations
    private const string AdminPasswordHash = "AQAAAAIAAYagAAAAENiQk5IFLxsI3vzGppLOS4O56DOxnsRaArsRQlh+qa2jhzyB7Qtznk23hZnlhIGsPw==";

    /// <summary>
    /// Seeds the system administrator user using HasData.
    /// </summary>
    public static void SeedSystemAdmin(ModelBuilder modelBuilder)
    {
        Guid adminId = GetUserId(AdminEmail);
        Guid systemAdminRoleId = GetRoleId(UserRole.SystemAdmin.ToString());

        ApplicationUser adminUser = new ApplicationUser
        {
            Id = adminId,
            UserName = AdminEmail,
            NormalizedUserName = AdminEmail.ToUpperInvariant(),
            Email = AdminEmail,
            NormalizedEmail = AdminEmail.ToUpperInvariant(),
            EmailConfirmed = true,
            PasswordHash = AdminPasswordHash,
            SecurityStamp = GetSecurityStamp(adminId),
            ConcurrencyStamp = GetConcurrencyStamp(adminId),
            LockoutEnabled = false,
            AccessFailedCount = 0,
            TwoFactorEnabled = false,
            PhoneNumberConfirmed = false
        };

        modelBuilder.Entity<ApplicationUser>().HasData(adminUser);

        // Seed user-role relationship
        IdentityUserRole<Guid> userRole = new IdentityUserRole<Guid>
        {
            UserId = adminId,
            RoleId = systemAdminRoleId
        };

        modelBuilder.Entity<IdentityUserRole<Guid>>().HasData(userRole);
    }

    /// <summary>
    /// Generates a deterministic GUID from a string using MD5 hashing.
    /// This ensures consistent user IDs across migrations.
    /// </summary>
    private static Guid GetUserId(string email)
    {
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes($"SystemAdmin_{email}"));
        return new Guid(hash);
    }

    /// <summary>
    /// Generates a deterministic GUID for a role ID using MD5 hashing.
    /// </summary>
    private static Guid GetRoleId(string roleName)
    {
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(roleName));
        return new Guid(hash);
    }

    /// <summary>
    /// Generates a deterministic security stamp from a user ID.
    /// </summary>
    private static string GetSecurityStamp(Guid userId)
    {
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes($"SecurityStamp_{userId}"));
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Generates a deterministic concurrency stamp from a user ID.
    /// </summary>
    private static string GetConcurrencyStamp(Guid userId)
    {
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes($"ConcurrencyStamp_{userId}"));
        return new Guid(hash).ToString();
    }
}

