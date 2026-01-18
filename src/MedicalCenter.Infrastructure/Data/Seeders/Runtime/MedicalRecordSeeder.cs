using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.MedicalRecords.Enums;
using MedicalCenter.Core.Aggregates.MedicalRecords.ValueObjects;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Infrastructure.Data.Seeders.Runtime.Fakers;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime;

/// <summary>
/// Seeder for MedicalRecord entities.
/// </summary>
public class MedicalRecordSeeder : IDatabaseSeeder
{
    private readonly ILogger<MedicalRecordSeeder> _logger;
    private readonly Faker _faker = new();

    public MedicalRecordSeeder(ILogger<MedicalRecordSeeder> logger)
    {
        _logger = logger;
    }

    public string EntityName => "MedicalRecord";

    public async Task SeedAsync(
        MedicalCenterDbContext context,
        int count,
        SeedingOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding {Count} {EntityName} entities...", count, EntityName);

        // Get all patients and practitioners
        var patients = await context.Patients
            .Where(p => p.IsActive)
            .Select(p => new { p.Id, p.FullName, p.Email })
            .ToListAsync(cancellationToken);

        var doctors = await context.Doctors
            .Where(d => d.IsActive)
            .Select(d => new { d.Id, d.FullName, d.Email, Role = UserRole.Doctor })
            .ToListAsync(cancellationToken);

        var healthcareStaff = await context.HealthcareStaff
            .Where(h => h.IsActive)
            .Select(h => new { h.Id, h.FullName, h.Email, Role = UserRole.HealthcareStaff })
            .ToListAsync(cancellationToken);

        var laboratories = await context.Laboratories
            .Where(l => l.IsActive)
            .Select(l => new { l.Id, l.FullName, l.Email, Role = UserRole.LabUser })
            .ToListAsync(cancellationToken);

        var imagingCenters = await context.ImagingCenters
            .Where(i => i.IsActive)
            .Select(i => new { i.Id, i.FullName, i.Email, Role = UserRole.ImagingUser })
            .ToListAsync(cancellationToken);

        var practitioners = doctors
            .Concat(healthcareStaff.Cast<dynamic>())
            .Concat(laboratories.Cast<dynamic>())
            .Concat(imagingCenters.Cast<dynamic>())
            .ToList();

        if (!patients.Any())
        {
            _logger.LogWarning("No patients found. Skipping medical record seeding.");
            return;
        }

        if (!practitioners.Any())
        {
            _logger.LogWarning("No practitioners found. Skipping medical record seeding.");
            return;
        }

        var medicalRecords = new List<MedicalRecord>();

        // Generate records per patient
        foreach (var patient in patients)
        {
            var recordsPerPatient = _faker.Random.Int(
                options.MedicalRecordsPerPatientMin,
                options.MedicalRecordsPerPatientMax);

            for (int i = 0; i < recordsPerPatient; i++)
            {
                // Pick a random practitioner
                var practitioner = _faker.PickRandom(practitioners);
                var practitionerVo = Practitioner.Create(
                    practitioner.FullName,
                    practitioner.Email,
                    practitioner.Role);

                // Pick a record type based on practitioner role
                var recordType = GetRecordTypeForPractitioner(practitioner.Role, _faker);

                // Generate a date in the past (last 5 years)
                var createdAt = _faker.Date.Between(DateTime.UtcNow.AddYears(-5), DateTime.UtcNow);

                var record = MedicalRecordFaker.Create(
                    patient.Id,
                    practitioner.Id,
                    practitionerVo,
                    recordType,
                    _faker,
                    createdAt);

                medicalRecords.Add(record);
            }
        }

        // If we need more records, generate additional ones
        if (medicalRecords.Count < count)
        {
            var additionalCount = count - medicalRecords.Count;
            for (int i = 0; i < additionalCount; i++)
            {
                var patient = _faker.PickRandom(patients);
                var practitioner = _faker.PickRandom(practitioners);
                var practitionerVo = Practitioner.Create(
                    practitioner.FullName,
                    practitioner.Email,
                    practitioner.Role);

                var recordType = GetRecordTypeForPractitioner(practitioner.Role, _faker);
                var createdAt = _faker.Date.Between(DateTime.UtcNow.AddYears(-5), DateTime.UtcNow);

                var record = MedicalRecordFaker.Create(
                    patient.Id,
                    practitioner.Id,
                    practitionerVo,
                    recordType,
                    _faker,
                    createdAt);

                medicalRecords.Add(record);
            }
        }

        context.MedicalRecords.AddRange(medicalRecords);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully seeded {Count} {EntityName} entities", medicalRecords.Count, EntityName);
    }

    private static RecordType GetRecordTypeForPractitioner(UserRole role, Faker faker)
    {
        return role switch
        {
            UserRole.Doctor => faker.PickRandom(new[]
            {
                RecordType.ConsultationNote,
                RecordType.Diagnosis,
                RecordType.Prescription,
                RecordType.TreatmentPlan
            }),
            UserRole.HealthcareStaff => faker.PickRandom(new[]
            {
                RecordType.ConsultationNote,
                RecordType.TreatmentPlan,
                RecordType.Other
            }),
            UserRole.LabUser => RecordType.LaboratoryResult,
            UserRole.ImagingUser => RecordType.ImagingReport,
            _ => faker.PickRandom<RecordType>()
        };
    }

    public async Task ClearAsync(MedicalCenterDbContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Clearing {EntityName} entities...", EntityName);

        var records = await context.MedicalRecords.ToListAsync(cancellationToken);
        context.MedicalRecords.RemoveRange(records);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cleared {EntityName} entities", EntityName);
    }
}

