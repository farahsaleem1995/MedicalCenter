using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Infrastructure.Data.Seeders.Runtime.Fakers;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime;

/// <summary>
/// Seeder for HealthcareStaff entities.
/// </summary>
public class HealthcareStaffSeeder : IDatabaseSeeder
{
    private readonly ILogger<HealthcareStaffSeeder> _logger;
    private readonly PasswordHasher<object> _passwordHasher;

    public HealthcareStaffSeeder(ILogger<HealthcareStaffSeeder> logger)
    {
        _logger = logger;
        _passwordHasher = new PasswordHasher<object>();
    }

    public string EntityName => "HealthcareStaff";

    public async Task SeedAsync(
        MedicalCenterDbContext context,
        int count,
        SeedingOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding {Count} {EntityName} entities...", count, EntityName);

        var faker = HealthcareStaffFaker.Create();
        var healthcareStaff = faker.Generate(count);
        var roleId = await GetRoleIdAsync(context, UserRole.HealthcareStaff, cancellationToken);

        var applicationUsers = new List<ApplicationUser>();
        var userRoles = new List<ApplicationUserRole>();

        foreach (var staff in healthcareStaff)
        {
            var appUser = new ApplicationUser
            {
                Id = staff.Id,
                UserName = staff.Email,
                NormalizedUserName = staff.Email.ToUpperInvariant(),
                Email = staff.Email,
                NormalizedEmail = staff.Email.ToUpperInvariant(),
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
                UserId = staff.Id,
                RoleId = roleId
            });
        }

        context.HealthcareStaff.AddRange(healthcareStaff);
        context.Users.AddRange(applicationUsers);
        context.Set<ApplicationUserRole>().AddRange(userRoles);

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully seeded {Count} {EntityName} entities", count, EntityName);
    }

    public async Task ClearAsync(MedicalCenterDbContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Clearing {EntityName} entities...", EntityName);

        var staffIds = await context.HealthcareStaff.Select(s => s.Id).ToListAsync(cancellationToken);
        
        if (staffIds.Any())
        {
            var userRoles = await context.Set<ApplicationUserRole>()
                .Where(ur => staffIds.Contains(ur.UserId))
                .ToListAsync(cancellationToken);
            context.Set<ApplicationUserRole>().RemoveRange(userRoles);

            var users = await context.Users
                .Where(u => staffIds.Contains(u.Id))
                .ToListAsync(cancellationToken);
            context.Users.RemoveRange(users);

            var staff = await context.HealthcareStaff
                .Where(s => staffIds.Contains(s.Id))
                .ToListAsync(cancellationToken);
            context.HealthcareStaff.RemoveRange(staff);

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

