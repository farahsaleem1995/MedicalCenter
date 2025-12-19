using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients.ChronicDiseases;

/// <summary>
/// List chronic diseases for a patient endpoint.
/// </summary>
public class ListChronicDiseasesEndpoint(IRepository<Patient> patientRepository)
    : Endpoint<ListChronicDiseasesRequest, ListChronicDiseasesResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<ChronicDiseasesGroup>();
        Policies(AuthorizationPolicies.CanViewMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "List patient chronic diseases";
            s.Description = "Returns all chronic diseases for a specific patient";
            s.Responses[200] = "Chronic diseases retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(ListChronicDiseasesRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        await Send.OkAsync(new ListChronicDiseasesResponse
        {
            ChronicDiseases = patient.ChronicDiseases.Select(cd => new ChronicDiseaseDto
            {
                Id = cd.Id,
                PatientId = cd.PatientId,
                Name = cd.Name,
                DiagnosisDate = cd.DiagnosisDate,
                Notes = cd.Notes,
                CreatedAt = cd.CreatedAt,
                UpdatedAt = cd.UpdatedAt
            }).ToList()
        }, ct);
    }
}

