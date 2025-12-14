using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Aggregates.Patient.Specifications;
using MedicalCenter.Core.Repositories;
using MedicalCenter.Infrastructure.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Get current patient's medical attributes endpoint.
/// </summary>
public class GetSelfMedicalAttributesEndpoint(IRepository<Patient> patientRepository)
    : EndpointWithoutRequest<GetSelfMedicalAttributesResponse>
{
    public override void Configure()
    {
        Get("/patients/self/medical-attributes");
        Group<PatientsGroup>();
        Policies(AuthorizationPolicies.RequirePatient);
        Summary(s =>
        {
            s.Summary = "Get current patient's medical attributes";
            s.Description = "Returns the authenticated patient's allergies, chronic diseases, medications, and surgeries";
            s.Responses[200] = "Medical attributes retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            ThrowError("Invalid user authentication", 401);
            return;
        }

        var specification = new PatientByIdSpecification(userId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        await Send.OkAsync(new GetSelfMedicalAttributesResponse
        {
            Allergies = patient.Allergies.Select(a => new AllergySummaryDto
            {
                Id = a.Id,
                Name = a.Name,
                Severity = a.Severity,
                Notes = a.Notes
            }).ToList(),
            ChronicDiseases = patient.ChronicDiseases.Select(cd => new ChronicDiseaseSummaryDto
            {
                Id = cd.Id,
                Name = cd.Name,
                DiagnosisDate = cd.DiagnosisDate,
                Notes = cd.Notes
            }).ToList(),
            Medications = patient.Medications.Select(m => new MedicationSummaryDto
            {
                Id = m.Id,
                Name = m.Name,
                Dosage = m.Dosage,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                Notes = m.Notes
            }).ToList(),
            Surgeries = patient.Surgeries.Select(s => new SurgerySummaryDto
            {
                Id = s.Id,
                Name = s.Name,
                Date = s.Date,
                Surgeon = s.Surgeon,
                Notes = s.Notes
            }).ToList()
        }, ct);
    }
}

