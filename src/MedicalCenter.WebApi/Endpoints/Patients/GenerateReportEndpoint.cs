using FastEndpoints;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.WebApi.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Generate medical report endpoint for patients.
/// </summary>
public class GenerateReportEndpoint(
    IMedicalReportService medicalReportService,
    IRepository<Patient> patientRepository,
    IUserContext userContext)
    : Endpoint<GenerateReportRequest>
{
    public override void Configure()
    {
        Get("/self/report");
        Group<PatientsGroup>();
        Policies(AuthorizationPolicies.RequirePatient);
        Summary(s =>
        {
            s.Summary = "Generate patient medical report";
            s.Description = "Generates a PDF medical report for the authenticated patient including medical attributes and records. Supports optional date filtering for medical records.";
            s.Responses[200] = "PDF report generated successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - user is not a patient";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(GenerateReportRequest req, CancellationToken ct)
    {
        var patientId = userContext.UserId;

        // Verify patient exists
        var specification = new PatientByIdSpecification(patientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        // Generate report
        var report = await medicalReportService.GenerateReportAsync(
            patientId,
            req.DateFrom,
            req.DateTo,
            ct);

        // Return PDF file
        await Send.StreamAsync(
            stream: report.PdfStream,
            fileName: report.FileName,
            contentType: report.ContentType,
            cancellation: ct);
    }
}

