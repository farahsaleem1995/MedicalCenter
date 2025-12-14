using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Aggregates.Patient.Specifications;
using MedicalCenter.Core.Repositories;
using MedicalCenter.Infrastructure.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients.Allergies;

/// <summary>
/// Create allergy for a patient endpoint.
/// </summary>
public class CreateAllergyEndpoint(
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<CreateAllergyRequest, CreateAllergyResponse>
{
    public override void Configure()
    {
        Post("/patients/{patientId}/allergies");
        Group<AllergiesGroup>();
        Policies(AuthorizationPolicies.CanModifyMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "Create allergy for patient";
            s.Description = "Creates a new allergy for a specific patient";
            s.Responses[201] = "Allergy created successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(CreateAllergyRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        var allergy = patient.AddAllergy(req.Name, req.Severity, req.Notes);

        await patientRepository.UpdateAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);

        Response = new CreateAllergyResponse
        {
            Id = allergy.Id,
            PatientId = allergy.PatientId,
            Name = allergy.Name,
            Severity = allergy.Severity,
            Notes = allergy.Notes,
            CreatedAt = allergy.CreatedAt
        };
    }
}

