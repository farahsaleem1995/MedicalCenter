using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Aggregates.Patient.Specifications;
using MedicalCenter.Core.Repositories;
using MedicalCenter.Infrastructure.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients.Allergies;

/// <summary>
/// List allergies for a patient endpoint.
/// </summary>
public class ListAllergiesEndpoint(IRepository<Patient> patientRepository)
    : Endpoint<ListAllergiesRequest, ListAllergiesResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<AllergiesGroup>();
        Policies(AuthorizationPolicies.CanViewMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "List patient allergies";
            s.Description = "Returns all allergies for a specific patient";
            s.Responses[200] = "Allergies retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(ListAllergiesRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        await Send.OkAsync(new ListAllergiesResponse
        {
            Allergies = patient.Allergies.Select(a => new AllergyDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                Name = a.Name,
                Severity = a.Severity,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList()
        }, ct);
    }
}

