using FastEndpoints;
using MedicalCenter.Infrastructure.Data.Seeders.Runtime;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MedicalCenter.WebApi.Endpoints.Dev;

/// <summary>
/// Development-only endpoint for seeding the database with fake data.
/// </summary>
public class SeedDatabaseEndpoint(
    DatabaseSeeder databaseSeeder,
    SeedingSummaryGenerator summaryGenerator,
    IWebHostEnvironment environment)
    : Endpoint<SeedDatabaseRequest>
{
    public override void Configure()
    {
        Post("/seed");
        Group<DevGroup>();
        AllowAnonymous(); // Development only - check environment in HandleAsync
        Summary(s =>
        {
            s.Summary = "Seed database with fake data";
            s.Description = "Seeds the database with fake data for testing and presentation purposes. Only available in Development environment. Returns the seeding summary as a markdown file download.";
            s.Responses[200] = "Database seeded successfully - returns SeedingSummary.md file";
            s.Responses[400] = "Bad request - invalid options";
            s.Responses[500] = "Internal server error during seeding";
        });
    }

    public override async Task HandleAsync(SeedDatabaseRequest req, CancellationToken ct)
    {
        // Only allow in Development environment
        if (!environment.IsDevelopment())
        {
            ThrowError("This endpoint is only available in Development environment", 403);
            return;
        }

        var options = new SeedingOptions
        {
            DoctorCount = req.DoctorCount ?? 20,
            HealthcareStaffCount = req.HealthcareStaffCount ?? 15,
            LaboratoryCount = req.LaboratoryCount ?? 5,
            ImagingCenterCount = req.ImagingCenterCount ?? 5,
            PatientCount = req.PatientCount ?? 100,
            MedicalRecordsPerPatientMin = req.MedicalRecordsPerPatientMin ?? 2,
            MedicalRecordsPerPatientMax = req.MedicalRecordsPerPatientMax ?? 10,
            ClearExistingData = req.ClearExistingData ?? false,
            DefaultPassword = req.DefaultPassword ?? "Test@123!"
        };

        await databaseSeeder.SeedAllAsync(options, ct);

        // Generate summary document as stream
        Stream summaryStream = await summaryGenerator.GenerateSummaryAsync(options, ct);

        await Send.StreamAsync(
            stream: summaryStream,
            fileName: "SeedingSummary.md",
            contentType: "text/markdown",
            cancellation: ct);
    }
}

/// <summary>
/// Request model for database seeding.
/// </summary>
public class SeedDatabaseRequest
{
    public int? DoctorCount { get; set; }
    public int? HealthcareStaffCount { get; set; }
    public int? LaboratoryCount { get; set; }
    public int? ImagingCenterCount { get; set; }
    public int? PatientCount { get; set; }
    public int? MedicalRecordsPerPatientMin { get; set; }
    public int? MedicalRecordsPerPatientMax { get; set; }
    public bool? ClearExistingData { get; set; }
    public string? DefaultPassword { get; set; }
}


