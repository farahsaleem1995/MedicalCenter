using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Aggregates.Patient.Specifications;
using MedicalCenter.Core.Common;
using MedicalCenter.Infrastructure.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients.Surgeries;

/// <summary>
/// List surgeries for a patient endpoint.
/// </summary>
public class ListSurgeriesEndpoint(IRepository<Patient> patientRepository)
    : Endpoint<ListSurgeriesRequest, ListSurgeriesResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<SurgeriesGroup>();
        Policies(AuthorizationPolicies.CanViewMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "List patient surgeries";
            s.Description = "Returns all surgeries for a specific patient";
            s.Responses[200] = "Surgeries retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(ListSurgeriesRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        await Send.OkAsync(new ListSurgeriesResponse
        {
            Surgeries = patient.Surgeries.Select(s => new SurgeryDto
            {
                Id = s.Id,
                PatientId = s.PatientId,
                Name = s.Name,
                Date = s.Date,
                Surgeon = s.Surgeon,
                Notes = s.Notes,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            }).ToList()
        }, ct);
    }
}

