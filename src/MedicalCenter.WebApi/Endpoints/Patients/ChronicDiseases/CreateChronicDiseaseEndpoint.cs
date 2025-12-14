using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Aggregates.Patient.Specifications;
using MedicalCenter.Core.Repositories;
using MedicalCenter.Infrastructure.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients.ChronicDiseases;

/// <summary>
/// Create chronic disease for a patient endpoint.
/// </summary>
public class CreateChronicDiseaseEndpoint(
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<CreateChronicDiseaseRequest, CreateChronicDiseaseResponse>
{
    public override void Configure()
    {
        Post("/patients/{patientId}/chronic-diseases");
        Group<ChronicDiseasesGroup>();
        Policies(AuthorizationPolicies.CanModifyMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "Create chronic disease for patient";
            s.Description = "Creates a new chronic disease for a specific patient";
            s.Responses[201] = "Chronic disease created successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(CreateChronicDiseaseRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        var chronicDisease = patient.AddChronicDisease(req.Name, req.DiagnosisDate, req.Notes);

        await patientRepository.UpdateAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);

        Response = new CreateChronicDiseaseResponse
        {
            Id = chronicDisease.Id,
            PatientId = chronicDisease.PatientId,
            Name = chronicDisease.Name,
            DiagnosisDate = chronicDisease.DiagnosisDate,
            Notes = chronicDisease.Notes,
            CreatedAt = chronicDisease.CreatedAt
        };
    }
}

