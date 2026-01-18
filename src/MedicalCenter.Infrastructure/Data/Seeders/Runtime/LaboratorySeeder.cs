using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MedicalCenter.Core.Aggregates.Laboratories;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Infrastructure.Data.Seeders.Runtime.Fakers;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime;

/// <summary>
/// Seeder for Laboratory entities.
/// </summary>
public class LaboratorySeeder : IDatabaseSeeder
{
    private readonly ILogger<LaboratorySeeder> _logger;
    private readonly PasswordHasher<object> _passwordHasher;

    public LaboratorySeeder(ILogger<LaboratorySeeder> logger)
    {
        _logger = logger;
        _passwordHasher = new PasswordHasher<object>();
    }

    public string EntityName => "Laboratory";

    public async Task SeedAsync(
        MedicalCenterDbContext context,
        int count,
        SeedingOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding {Count} {EntityName} entities...", count, EntityName);

        var faker = LaboratoryFaker.Create();
        var laboratories = faker.Generate(count);
        var roleId = await GetRoleIdAsync(context, UserRole.LabUser, cancellationToken);

        var applicationUsers = new List<ApplicationUser>();
        var userRoles = new List<ApplicationUserRole>();

        foreach (var lab in laboratories)
        {
            var appUser = new ApplicationUser
            {
                Id = lab.Id,
                UserName = lab.Email,
                NormalizedUserName = lab.Email.ToUpperInvariant(),
                Email = lab.Email,
                NormalizedEmail = lab.Email.ToUpperInvariant(),
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
                UserId = lab.Id,
                RoleId = roleId
            });
        }

        context.Laboratories.AddRange(laboratories);
        context.Users.AddRange(applicationUsers);
        context.Set<ApplicationUserRole>().AddRange(userRoles);

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully seeded {Count} {EntityName} entities", count, EntityName);
    }

    public async Task ClearAsync(MedicalCenterDbContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Clearing {EntityName} entities...", EntityName);

        var labIds = await context.Laboratories.Select(l => l.Id).ToListAsync(cancellationToken);
        
        if (labIds.Any())
        {
            var userRoles = await context.Set<ApplicationUserRole>()
                .Where(ur => labIds.Contains(ur.UserId))
                .ToListAsync(cancellationToken);
            context.Set<ApplicationUserRole>().RemoveRange(userRoles);

            var users = await context.Users
                .Where(u => labIds.Contains(u.Id))
                .ToListAsync(cancellationToken);
            context.Users.RemoveRange(users);

            var labs = await context.Laboratories
                .Where(l => labIds.Contains(l.Id))
                .ToListAsync(cancellationToken);
            context.Laboratories.RemoveRange(labs);

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

