using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MedicalCenter.Core.Aggregates.ImagingCenters;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Infrastructure.Data.Seeders.Runtime.Fakers;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime;

/// <summary>
/// Seeder for ImagingCenter entities.
/// </summary>
public class ImagingCenterSeeder : IDatabaseSeeder
{
    private readonly ILogger<ImagingCenterSeeder> _logger;
    private readonly PasswordHasher<object> _passwordHasher;

    public ImagingCenterSeeder(ILogger<ImagingCenterSeeder> logger)
    {
        _logger = logger;
        _passwordHasher = new PasswordHasher<object>();
    }

    public string EntityName => "ImagingCenter";

    public async Task SeedAsync(
        MedicalCenterDbContext context,
        int count,
        SeedingOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding {Count} {EntityName} entities...", count, EntityName);

        var faker = ImagingCenterFaker.Create();
        var imagingCenters = faker.Generate(count);
        var roleId = await GetRoleIdAsync(context, UserRole.ImagingUser, cancellationToken);

        var applicationUsers = new List<ApplicationUser>();
        var userRoles = new List<ApplicationUserRole>();

        foreach (var center in imagingCenters)
        {
            var appUser = new ApplicationUser
            {
                Id = center.Id,
                UserName = center.Email,
                NormalizedUserName = center.Email.ToUpperInvariant(),
                Email = center.Email,
                NormalizedEmail = center.Email.ToUpperInvariant(),
                EmailConfirmed = true,
                PasswordHash = _passwordHasher.HashPassword(null!, options.DefaultPassword),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                LockoutEnabled = true,
                AccessFailedCount = 0,
                TwoFactorEnabled = false,
                PhoneNumberConfirmed = false
            };

            applicationUsers.Add(appUser);

            userRoles.Add(new ApplicationUserRole
            {
                UserId = center.Id,
                RoleId = roleId
            });
        }

        context.ImagingCenters.AddRange(imagingCenters);
        context.Users.AddRange(applicationUsers);
        context.Set<ApplicationUserRole>().AddRange(userRoles);

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully seeded {Count} {EntityName} entities", count, EntityName);
    }

    public async Task ClearAsync(MedicalCenterDbContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Clearing {EntityName} entities...", EntityName);

        var centerIds = await context.ImagingCenters.Select(i => i.Id).ToListAsync(cancellationToken);
        
        if (centerIds.Any())
        {
            var userRoles = await context.Set<ApplicationUserRole>()
                .Where(ur => centerIds.Contains(ur.UserId))
                .ToListAsync(cancellationToken);
            context.Set<ApplicationUserRole>().RemoveRange(userRoles);

            var users = await context.Users
                .Where(u => centerIds.Contains(u.Id))
                .ToListAsync(cancellationToken);
            context.Users.RemoveRange(users);

            var centers = await context.ImagingCenters
                .Where(i => centerIds.Contains(i.Id))
                .ToListAsync(cancellationToken);
            context.ImagingCenters.RemoveRange(centers);

            await context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Cleared {EntityName} entities", EntityName);
    }

    private static async Task<Guid> GetRoleIdAsync(MedicalCenterDbContext context, UserRole role, CancellationToken cancellationToken)
    {
        var roleName = role.ToString();
        var applicationRole = await context.Roles
            .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);

        if (applicationRole == null)
        {
            throw new InvalidOperationException($"Role '{roleName}' not found. Ensure roles are seeded first.");
        }

        return applicationRole.Id;
    }
}

