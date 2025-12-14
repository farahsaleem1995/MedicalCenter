using Microsoft.EntityFrameworkCore;
using MedicalCenter.Infrastructure.Data.Seeders;

namespace MedicalCenter.Infrastructure.Data.Seeders;

/// <summary>
/// Extension methods for ModelBuilder to seed database data.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Seeds all initial data for the Medical Center database.
    /// </summary>
    public static void SeedData(this ModelBuilder modelBuilder)
    {
        RoleSeeder.SeedRoles(modelBuilder);
        SystemAdminSeeder.SeedSystemAdmin(modelBuilder);
        // Add other seeders here as needed:
        // ConfigurationSeeder.SeedConfiguration(modelBuilder);
    }
}

