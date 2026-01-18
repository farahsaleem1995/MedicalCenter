using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MedicalCenter.Core.Aggregates.Doctors;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Core.Aggregates.ImagingCenters;
using MedicalCenter.Core.Aggregates.Laboratories;
using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Entities;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.Infrastructure.Identity;
using System.Text;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime;

/// <summary>
/// Generates a summary document after seeding operations.
/// </summary>
public class SeedingSummaryGenerator
{
    private readonly MedicalCenterDbContext _context;
    private readonly ILogger<SeedingSummaryGenerator> _logger;

    public SeedingSummaryGenerator(
        MedicalCenterDbContext context,
        ILogger<SeedingSummaryGenerator> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Generates the seeding summary document content as a stream.
    /// </summary>
    public async Task<Stream> GenerateSummaryAsync(SeedingOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating seeding summary document");

        SeedingSummary summary = await BuildSummaryAsync(options, cancellationToken);
        string markdown = FormatAsMarkdown(summary);

        // Convert string to stream
        MemoryStream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
        await writer.WriteAsync(markdown);
        await writer.FlushAsync();
        stream.Position = 0;

        _logger.LogInformation("Seeding summary document generated successfully");

        return stream;
    }

    private async Task<SeedingSummary> BuildSummaryAsync(SeedingOptions options, CancellationToken cancellationToken)
    {
        var summary = new SeedingSummary
        {
            SeedingTimestamp = DateTime.UtcNow,
            Options = new SeedingOptionsSummary
            {
                DoctorCount = options.DoctorCount,
                HealthcareStaffCount = options.HealthcareStaffCount,
                LaboratoryCount = options.LaboratoryCount,
                ImagingCenterCount = options.ImagingCenterCount,
                PatientCount = options.PatientCount,
                MedicalRecordsPerPatientMin = options.MedicalRecordsPerPatientMin,
                MedicalRecordsPerPatientMax = options.MedicalRecordsPerPatientMax,
                DefaultPassword = options.DefaultPassword
            }
        };

        // Get all users with their credentials
        var doctors = await _context.Doctors
            .Select(d => new { d.Id, d.FullName, d.Email, d.LicenseNumber, d.Specialty })
            .ToListAsync(cancellationToken);

        var healthcareStaff = await _context.HealthcareStaff
            .Select(h => new { h.Id, h.FullName, h.Email, h.OrganizationName, h.Department })
            .ToListAsync(cancellationToken);

        var laboratories = await _context.Laboratories
            .Select(l => new { l.Id, l.FullName, l.Email, l.LabName, l.LicenseNumber })
            .ToListAsync(cancellationToken);

        var imagingCenters = await _context.ImagingCenters
            .Select(i => new { i.Id, i.FullName, i.Email, i.CenterName, i.LicenseNumber })
            .ToListAsync(cancellationToken);

        var patients = await _context.Patients
            .Select(p => new { p.Id, p.FullName, p.Email, p.NationalId, p.DateOfBirth })
            .ToListAsync(cancellationToken);

        // Build user credentials
        summary.Users.AddRange(doctors.Select(d => new UserCredential
        {
            Email = d.Email,
            Password = options.DefaultPassword,
            Role = UserRole.Doctor.ToString(),
            FullName = d.FullName,
            UserId = d.Id,
            AdditionalInfo = $"License: {d.LicenseNumber}, Specialty: {d.Specialty}"
        }));

        summary.Users.AddRange(healthcareStaff.Select(h => new UserCredential
        {
            Email = h.Email,
            Password = options.DefaultPassword,
            Role = UserRole.HealthcareStaff.ToString(),
            FullName = h.FullName,
            UserId = h.Id,
            AdditionalInfo = $"Organization: {h.OrganizationName}, Department: {h.Department}"
        }));

        summary.Users.AddRange(laboratories.Select(l => new UserCredential
        {
            Email = l.Email,
            Password = options.DefaultPassword,
            Role = UserRole.LabUser.ToString(),
            FullName = l.FullName,
            UserId = l.Id,
            AdditionalInfo = $"Lab: {l.LabName}, License: {l.LicenseNumber}"
        }));

        summary.Users.AddRange(imagingCenters.Select(i => new UserCredential
        {
            Email = i.Email,
            Password = options.DefaultPassword,
            Role = UserRole.ImagingUser.ToString(),
            FullName = i.FullName,
            UserId = i.Id,
            AdditionalInfo = $"Center: {i.CenterName}, License: {i.LicenseNumber}"
        }));

        summary.Users.AddRange(patients.Select(p => new UserCredential
        {
            Email = p.Email,
            Password = options.DefaultPassword,
            Role = UserRole.Patient.ToString(),
            FullName = p.FullName,
            UserId = p.Id,
            AdditionalInfo = $"National ID: {p.NationalId}, DOB: {p.DateOfBirth:yyyy-MM-dd}"
        }));

        // Build data summary
        summary.DataSummary = new DataSummary
        {
            Patients = patients.Count,
            Doctors = doctors.Count,
            HealthcareStaff = healthcareStaff.Count,
            Laboratories = laboratories.Count,
            ImagingCenters = imagingCenters.Count,
            MedicalRecords = await _context.MedicalRecords.CountAsync(cancellationToken),
            TotalAllergies = await _context.Set<Allergy>().CountAsync(cancellationToken),
            TotalChronicDiseases = await _context.Set<ChronicDisease>().CountAsync(cancellationToken),
            TotalMedications = await _context.Set<Medication>().CountAsync(cancellationToken),
            TotalSurgeries = await _context.Set<Surgery>().CountAsync(cancellationToken)
        };

        // Medical records distribution
        var recordsByType = await _context.MedicalRecords
            .GroupBy(r => r.RecordType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        summary.DataSummary.MedicalRecordsByType = recordsByType.ToDictionary(
            r => r.Type.ToString(),
            r => r.Count);

        // Date ranges
        if (patients.Any())
        {
            summary.DataSummary.PatientDateOfBirthRange = new DateRange
            {
                Min = patients.Min(p => p.DateOfBirth),
                Max = patients.Max(p => p.DateOfBirth)
            };
        }

        var medicalRecordDates = await _context.MedicalRecords
            .Select(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        if (medicalRecordDates.Any())
        {
            summary.DataSummary.MedicalRecordDateRange = new DateRange
            {
                Min = medicalRecordDates.Min(),
                Max = medicalRecordDates.Max()
            };
        }

        // Sample IDs
        summary.SampleIds = new SampleIds
        {
            SamplePatientIds = patients.Take(5).Select(p => p.Id).ToList(),
            SampleDoctorIds = doctors.Take(3).Select(d => d.Id).ToList(),
            SampleMedicalRecordIds = await _context.MedicalRecords
                .Take(5)
                .Select(r => r.Id)
                .ToListAsync(cancellationToken)
        };

        return summary;
    }

    private static string FormatAsMarkdown(SeedingSummary summary)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Database Seeding Summary");
        sb.AppendLine();
        sb.AppendLine($"**Generated:** {summary.SeedingTimestamp:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        // Options
        sb.AppendLine("## Seeding Options");
        sb.AppendLine();
        sb.AppendLine($"- Doctors: {summary.Options.DoctorCount}");
        sb.AppendLine($"- Healthcare Staff: {summary.Options.HealthcareStaffCount}");
        sb.AppendLine($"- Laboratories: {summary.Options.LaboratoryCount}");
        sb.AppendLine($"- Imaging Centers: {summary.Options.ImagingCenterCount}");
        sb.AppendLine($"- Patients: {summary.Options.PatientCount}");
        sb.AppendLine($"- Medical Records per Patient: {summary.Options.MedicalRecordsPerPatientMin}-{summary.Options.MedicalRecordsPerPatientMax}");
        sb.AppendLine($"- Default Password: `{summary.Options.DefaultPassword}`");
        sb.AppendLine();

        // Data Summary
        sb.AppendLine("## Data Summary");
        sb.AppendLine();
        sb.AppendLine($"- **Patients:** {summary.DataSummary.Patients}");
        sb.AppendLine($"- **Doctors:** {summary.DataSummary.Doctors}");
        sb.AppendLine($"- **Healthcare Staff:** {summary.DataSummary.HealthcareStaff}");
        sb.AppendLine($"- **Laboratories:** {summary.DataSummary.Laboratories}");
        sb.AppendLine($"- **Imaging Centers:** {summary.DataSummary.ImagingCenters}");
        sb.AppendLine($"- **Medical Records:** {summary.DataSummary.MedicalRecords}");
        sb.AppendLine($"- **Total Allergies:** {summary.DataSummary.TotalAllergies}");
        sb.AppendLine($"- **Total Chronic Diseases:** {summary.DataSummary.TotalChronicDiseases}");
        sb.AppendLine($"- **Total Medications:** {summary.DataSummary.TotalMedications}");
        sb.AppendLine($"- **Total Surgeries:** {summary.DataSummary.TotalSurgeries}");
        sb.AppendLine();

        if (summary.DataSummary.MedicalRecordsByType.Any())
        {
            sb.AppendLine("### Medical Records by Type");
            sb.AppendLine();
            foreach (var kvp in summary.DataSummary.MedicalRecordsByType)
            {
                sb.AppendLine($"- **{kvp.Key}:** {kvp.Value}");
            }
            sb.AppendLine();
        }

        if (summary.DataSummary.PatientDateOfBirthRange != null)
        {
            sb.AppendLine($"### Patient Date of Birth Range");
            sb.AppendLine($"- **Min:** {summary.DataSummary.PatientDateOfBirthRange.Min:yyyy-MM-dd}");
            sb.AppendLine($"- **Max:** {summary.DataSummary.PatientDateOfBirthRange.Max:yyyy-MM-dd}");
            sb.AppendLine();
        }

        if (summary.DataSummary.MedicalRecordDateRange != null)
        {
            sb.AppendLine($"### Medical Record Date Range");
            sb.AppendLine($"- **Min:** {summary.DataSummary.MedicalRecordDateRange.Min:yyyy-MM-dd}");
            sb.AppendLine($"- **Max:** {summary.DataSummary.MedicalRecordDateRange.Max:yyyy-MM-dd}");
            sb.AppendLine();
        }

        // User Credentials
        sb.AppendLine("## User Credentials");
        sb.AppendLine();
        sb.AppendLine("> **Note:** All seeded users have the same default password for testing purposes.");
        sb.AppendLine();

        var usersByRole = summary.Users.GroupBy(u => u.Role).OrderBy(g => g.Key);

        foreach (var roleGroup in usersByRole)
        {
            sb.AppendLine($"### {roleGroup.Key}");
            sb.AppendLine();
            sb.AppendLine("| Email | Password | Full Name | Additional Info |");
            sb.AppendLine("|-------|----------|-----------|-----------------|");

            foreach (var user in roleGroup)
            {
                sb.AppendLine($"| {user.Email} | `{user.Password}` | {user.FullName} | {user.AdditionalInfo} |");
            }

            sb.AppendLine();
        }

        // Sample IDs
        sb.AppendLine("## Sample IDs for Testing");
        sb.AppendLine();
        sb.AppendLine("### Sample Patient IDs");
        foreach (var id in summary.SampleIds.SamplePatientIds)
        {
            sb.AppendLine($"- `{id}`");
        }
        sb.AppendLine();

        sb.AppendLine("### Sample Doctor IDs");
        foreach (var id in summary.SampleIds.SampleDoctorIds)
        {
            sb.AppendLine($"- `{id}`");
        }
        sb.AppendLine();

        sb.AppendLine("### Sample Medical Record IDs");
        foreach (var id in summary.SampleIds.SampleMedicalRecordIds)
        {
            sb.AppendLine($"- `{id}`");
        }
        sb.AppendLine();

        return sb.ToString();
    }

    private class SeedingSummary
    {
        public DateTime SeedingTimestamp { get; set; }
        public SeedingOptionsSummary Options { get; set; } = null!;
        public List<UserCredential> Users { get; set; } = new();
        public DataSummary DataSummary { get; set; } = null!;
        public SampleIds SampleIds { get; set; } = null!;
    }

    private class SeedingOptionsSummary
    {
        public int DoctorCount { get; set; }
        public int HealthcareStaffCount { get; set; }
        public int LaboratoryCount { get; set; }
        public int ImagingCenterCount { get; set; }
        public int PatientCount { get; set; }
        public int MedicalRecordsPerPatientMin { get; set; }
        public int MedicalRecordsPerPatientMax { get; set; }
        public string DefaultPassword { get; set; } = string.Empty;
    }

    private class UserCredential
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string AdditionalInfo { get; set; } = string.Empty;
    }

    private class DataSummary
    {
        public int Patients { get; set; }
        public int Doctors { get; set; }
        public int HealthcareStaff { get; set; }
        public int Laboratories { get; set; }
        public int ImagingCenters { get; set; }
        public int MedicalRecords { get; set; }
        public int TotalAllergies { get; set; }
        public int TotalChronicDiseases { get; set; }
        public int TotalMedications { get; set; }
        public int TotalSurgeries { get; set; }
        public Dictionary<string, int> MedicalRecordsByType { get; set; } = new();
        public DateRange? PatientDateOfBirthRange { get; set; }
        public DateRange? MedicalRecordDateRange { get; set; }
    }

    private class DateRange
    {
        public DateTime Min { get; set; }
        public DateTime Max { get; set; }
    }

    private class SampleIds
    {
        public List<Guid> SamplePatientIds { get; set; } = new();
        public List<Guid> SampleDoctorIds { get; set; } = new();
        public List<Guid> SampleMedicalRecordIds { get; set; } = new();
    }
}

