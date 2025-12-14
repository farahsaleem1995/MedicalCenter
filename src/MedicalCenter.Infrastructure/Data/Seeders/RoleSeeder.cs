using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedicalCenter.Core.Enums;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Seeders;

/// <summary>
/// Seeds Identity roles into the database using EF Core HasData.
/// </summary>
public static class RoleSeeder
{
    /// <summary>
    /// Seeds all Identity roles defined in the UserRole enum.
    /// </summary>
    public static void SeedRoles(ModelBuilder modelBuilder)
    {
        var roles = Enum.GetValues<UserRole>()
            .Select(role => role.ToString())
            .ToList();

        var roleEntities = roles.Select(roleName => new ApplicationRole
        {
            Id = GetRoleId(roleName),
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant(),
            ConcurrencyStamp = GetRoleId($"{roleName}_ConcurrencyStamp").ToString()
        }).ToArray();

        modelBuilder.Entity<ApplicationRole>().HasData(roleEntities);
    }

    /// <summary>
    /// Generates a deterministic GUID from a string using MD5 hashing.
    /// This ensures consistent role IDs across migrations.
    /// </summary>
    private static Guid GetRoleId(string roleName)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(roleName));
        return new Guid(hash);
    }
}

