using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MedicalCenter.Infrastructure.Data;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime;

/// <summary>
/// Main orchestrator for database seeding operations.
/// Coordinates all seeders in the correct dependency order.
/// </summary>
public class DatabaseSeeder
{
    private readonly MedicalCenterDbContext _context;
    private readonly IEnumerable<IDatabaseSeeder> _seeders;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        MedicalCenterDbContext context,
        IEnumerable<IDatabaseSeeder> seeders,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _seeders = seeders;
        _logger = logger;
    }

    /// <summary>
    /// Seeds all entities according to the provided options.
    /// </summary>
    public async Task SeedAllAsync(SeedingOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting database seeding with options: {@Options}", options);

        try
        {
            // Clear existing data if requested
            if (options.ClearExistingData)
            {
                _logger.LogInformation("Clearing existing seeded data...");
                await ClearAllAsync(cancellationToken);
            }

            // Seed in dependency order:
            // 1. Practitioners (Doctors, HealthcareStaff, Labs, Imaging)
            var doctorSeeder = _seeders.OfType<DoctorSeeder>().FirstOrDefault();
            if (doctorSeeder != null && options.DoctorCount > 0)
            {
                await doctorSeeder.SeedAsync(_context, options.DoctorCount, options, cancellationToken);
            }

            var healthcareStaffSeeder = _seeders.OfType<HealthcareStaffSeeder>().FirstOrDefault();
            if (healthcareStaffSeeder != null && options.HealthcareStaffCount > 0)
            {
                await healthcareStaffSeeder.SeedAsync(_context, options.HealthcareStaffCount, options, cancellationToken);
            }

            var laboratorySeeder = _seeders.OfType<LaboratorySeeder>().FirstOrDefault();
            if (laboratorySeeder != null && options.LaboratoryCount > 0)
            {
                await laboratorySeeder.SeedAsync(_context, options.LaboratoryCount, options, cancellationToken);
            }

            var imagingCenterSeeder = _seeders.OfType<ImagingCenterSeeder>().FirstOrDefault();
            if (imagingCenterSeeder != null && options.ImagingCenterCount > 0)
            {
                await imagingCenterSeeder.SeedAsync(_context, options.ImagingCenterCount, options, cancellationToken);
            }

            // 2. Patients (depends on nothing, but needed for medical records)
            var patientSeeder = _seeders.OfType<PatientSeeder>().FirstOrDefault();
            if (patientSeeder != null && options.PatientCount > 0)
            {
                await patientSeeder.SeedAsync(_context, options.PatientCount, options, cancellationToken);
            }

            // 3. Medical Records (depends on Patients and Practitioners)
            var medicalRecordSeeder = _seeders.OfType<MedicalRecordSeeder>().FirstOrDefault();
            if (medicalRecordSeeder != null)
            {
                // Calculate approximate number of records based on patients
                var estimatedRecordCount = options.PatientCount * 
                    ((options.MedicalRecordsPerPatientMin + options.MedicalRecordsPerPatientMax) / 2);
                await medicalRecordSeeder.SeedAsync(_context, estimatedRecordCount, options, cancellationToken);
            }

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database seeding");
            throw;
        }
    }

    /// <summary>
    /// Clears all seeded data in reverse dependency order.
    /// </summary>
    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Clearing all seeded data...");

        // Clear in reverse dependency order
        var medicalRecordSeeder = _seeders.OfType<MedicalRecordSeeder>().FirstOrDefault();
        if (medicalRecordSeeder != null)
        {
            await medicalRecordSeeder.ClearAsync(_context, cancellationToken);
        }

        var patientSeeder = _seeders.OfType<PatientSeeder>().FirstOrDefault();
        if (patientSeeder != null)
        {
            await patientSeeder.ClearAsync(_context, cancellationToken);
        }

        var imagingCenterSeeder = _seeders.OfType<ImagingCenterSeeder>().FirstOrDefault();
        if (imagingCenterSeeder != null)
        {
            await imagingCenterSeeder.ClearAsync(_context, cancellationToken);
        }

        var laboratorySeeder = _seeders.OfType<LaboratorySeeder>().FirstOrDefault();
        if (laboratorySeeder != null)
        {
            await laboratorySeeder.ClearAsync(_context, cancellationToken);
        }

        var healthcareStaffSeeder = _seeders.OfType<HealthcareStaffSeeder>().FirstOrDefault();
        if (healthcareStaffSeeder != null)
        {
            await healthcareStaffSeeder.ClearAsync(_context, cancellationToken);
        }

        var doctorSeeder = _seeders.OfType<DoctorSeeder>().FirstOrDefault();
        if (doctorSeeder != null)
        {
            await doctorSeeder.ClearAsync(_context, cancellationToken);
        }

        _logger.LogInformation("All seeded data cleared");
    }
}

