using MedicalCenter.Infrastructure.Data;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime;

/// <summary>
/// Interface for database seeders.
/// </summary>
public interface IDatabaseSeeder
{
    /// <summary>
    /// Seeds the database with fake data.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="count">The number of entities to seed.</param>
    /// <param name="options">Additional seeding options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SeedAsync(
        MedicalCenterDbContext context, 
        int count, 
        SeedingOptions options,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the name of the entity being seeded.
    /// </summary>
    string EntityName { get; }
    
    /// <summary>
    /// Clears existing seeded data for this entity type.
    /// </summary>
    Task ClearAsync(MedicalCenterDbContext context, CancellationToken cancellationToken = default);
}

