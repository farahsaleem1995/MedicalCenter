using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MedicalCenter.Core.Aggregates.Doctors;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Infrastructure.Data.Seeders.Runtime.Fakers;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime;

/// <summary>
/// Seeder for Doctor entities.
/// </summary>
public class DoctorSeeder : IDatabaseSeeder
{
    private readonly ILogger<DoctorSeeder> _logger;
    private readonly PasswordHasher<object> _passwordHasher;

    public DoctorSeeder(ILogger<DoctorSeeder> logger)
    {
        _logger = logger;
        _passwordHasher = new PasswordHasher<object>();
    }

    public string EntityName => "Doctor";

    public async Task SeedAsync(
        MedicalCenterDbContext context,
        int count,
        SeedingOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding {Count} {EntityName} entities...", count, EntityName);

        var faker = DoctorFaker.Create();
        var doctors = faker.Generate(count);
        var roleId = await GetRoleIdAsync(context, UserRole.Doctor, cancellationToken);

        var applicationUsers = new List<ApplicationUser>();
        var userRoles = new List<ApplicationUserRole>();

        foreach (var doctor in doctors)
        {
            var appUser = new ApplicationUser
            {
                Id = doctor.Id,
                UserName = doctor.Email,
                NormalizedUserName = doctor.Email.ToUpperInvariant(),
                Email = doctor.Email,
                NormalizedEmail = doctor.Email.ToUpperInvariant(),
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
                UserId = doctor.Id,
                RoleId = roleId
            });
        }

        context.Doctors.AddRange(doctors);
        context.Users.AddRange(applicationUsers);
        context.Set<ApplicationUserRole>().AddRange(userRoles);

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully seeded {Count} {EntityName} entities", count, EntityName);
    }

    public async Task ClearAsync(MedicalCenterDbContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Clearing {EntityName} entities...", EntityName);

        var doctorIds = await context.Doctors.Select(d => d.Id).ToListAsync(cancellationToken);
        
        if (doctorIds.Any())
        {
            // Delete user roles
            var userRoles = await context.Set<ApplicationUserRole>()
                .Where(ur => doctorIds.Contains(ur.UserId))
                .ToListAsync(cancellationToken);
            context.Set<ApplicationUserRole>().RemoveRange(userRoles);

            // Delete Identity users
            var users = await context.Users
                .Where(u => doctorIds.Contains(u.Id))
                .ToListAsync(cancellationToken);
            context.Users.RemoveRange(users);

            // Delete domain entities
            var doctors = await context.Doctors
                .Where(d => doctorIds.Contains(d.Id))
                .ToListAsync(cancellationToken);
            context.Doctors.RemoveRange(doctors);

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

