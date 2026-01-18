using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Infrastructure.Data.Seeders.Runtime.Fakers;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime;

/// <summary>
/// Seeder for Patient entities with medical attributes.
/// </summary>
public class PatientSeeder : IDatabaseSeeder
{
    private readonly ILogger<PatientSeeder> _logger;
    private readonly PasswordHasher<object> _passwordHasher;
    private readonly Faker _faker = new();

    public PatientSeeder(ILogger<PatientSeeder> logger)
    {
        _logger = logger;
        _passwordHasher = new PasswordHasher<object>();
    }

    public string EntityName => "Patient";

    public async Task SeedAsync(
        MedicalCenterDbContext context,
        int count,
        SeedingOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding {Count} {EntityName} entities with medical attributes...", count, EntityName);

        var faker = PatientFaker.Create();
        var patients = faker.Generate(count);
        var roleId = await GetRoleIdAsync(context, UserRole.Patient, cancellationToken);

        var applicationUsers = new List<ApplicationUser>();
        var userRoles = new List<ApplicationUserRole>();

        foreach (var patient in patients)
        {
            // Add medical attributes using domain methods
            AddMedicalAttributes(patient);

            var appUser = new ApplicationUser
            {
                Id = patient.Id,
                UserName = patient.Email,
                NormalizedUserName = patient.Email.ToUpperInvariant(),
                Email = patient.Email,
                NormalizedEmail = patient.Email.ToUpperInvariant(),
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
                UserId = patient.Id,
                RoleId = roleId
            });
        }

        context.Patients.AddRange(patients);
        context.Users.AddRange(applicationUsers);
        context.Set<ApplicationUserRole>().AddRange(userRoles);

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully seeded {Count} {EntityName} entities", count, EntityName);
    }

    private void AddMedicalAttributes(Patient patient)
    {
        // Add allergies (0-5 per patient)
        var allergyCount = _faker.Random.Int(0, 5);
        for (int i = 0; i < allergyCount; i++)
        {
            var allergyName = MedicalAttributeFaker.GetRandomAllergy(_faker);
            var severity = MedicalAttributeFaker.GetRandomAllergySeverity(_faker);
            var notes = _faker.Random.Bool(0.3f) ? _faker.Lorem.Sentence() : null;
            patient.AddAllergy(allergyName, severity, notes);
        }

        // Add chronic diseases (0-3 per patient)
        var diseaseCount = _faker.Random.Int(0, 3);
        for (int i = 0; i < diseaseCount; i++)
        {
            var diseaseName = MedicalAttributeFaker.GetRandomChronicDisease(_faker);
            var diagnosisDate = _faker.Date.Between(DateTime.UtcNow.AddYears(-20), DateTime.UtcNow.AddMonths(-1));
            var notes = _faker.Random.Bool(0.4f) ? _faker.Lorem.Sentence() : null;
            patient.AddChronicDisease(diseaseName, diagnosisDate, notes);
        }

        // Add medications (0-6 per patient)
        var medicationCount = _faker.Random.Int(0, 6);
        for (int i = 0; i < medicationCount; i++)
        {
            var medicationName = MedicalAttributeFaker.GetRandomMedication(_faker);
            var dosage = MedicalAttributeFaker.GetRandomDosage(_faker);
            var startDate = _faker.Date.Between(DateTime.UtcNow.AddYears(-5), DateTime.UtcNow.AddMonths(-1));
            DateTime? endDate = _faker.Random.Bool(0.4f) ? _faker.Date.Between(startDate, DateTime.UtcNow) : (DateTime?)null;
            var notes = _faker.Random.Bool(0.3f) ? _faker.Lorem.Sentence() : null;
            patient.AddMedication(medicationName, dosage, startDate, endDate, notes);
        }

        // Add surgeries (0-4 per patient)
        var surgeryCount = _faker.Random.Int(0, 4);
        for (int i = 0; i < surgeryCount; i++)
        {
            var surgeryName = MedicalAttributeFaker.GetRandomSurgery(_faker);
            var surgeryDate = _faker.Date.Between(DateTime.UtcNow.AddYears(-30), DateTime.UtcNow.AddMonths(-1));
            var surgeon = MedicalAttributeFaker.GetRandomSurgeonName(_faker);
            var notes = _faker.Random.Bool(0.3f) ? _faker.Lorem.Sentence() : null;
            patient.AddSurgery(surgeryName, surgeryDate, surgeon, notes);
        }
    }

    public async Task ClearAsync(MedicalCenterDbContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Clearing {EntityName} entities...", EntityName);

        var patientIds = await context.Patients.Select(p => p.Id).ToListAsync(cancellationToken);
        
        if (patientIds.Any())
        {
            // Medical attributes are cascade deleted, but we need to delete user roles and identity users first
            var userRoles = await context.Set<ApplicationUserRole>()
                .Where(ur => patientIds.Contains(ur.UserId))
                .ToListAsync(cancellationToken);
            context.Set<ApplicationUserRole>().RemoveRange(userRoles);

            var users = await context.Users
                .Where(u => patientIds.Contains(u.Id))
                .ToListAsync(cancellationToken);
            context.Users.RemoveRange(users);

            var patients = await context.Patients
                .Where(p => patientIds.Contains(p.Id))
                .ToListAsync(cancellationToken);
            context.Patients.RemoveRange(patients);

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

