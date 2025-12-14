using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Aggregates.Patient.Specifications;
using MedicalCenter.Core.Repositories;
using MedicalCenter.Infrastructure.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients.Allergies;

/// <summary>
/// Update allergy endpoint.
/// </summary>
public class UpdateAllergyEndpoint(
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<UpdateAllergyRequest, UpdateAllergyResponse>
{
    public override void Configure()
    {
        Put("/patients/{patientId}/allergies/{allergyId}");
        Group<AllergiesGroup>();
        Policies(AuthorizationPolicies.CanModifyMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "Update allergy";
            s.Description = "Updates an existing allergy for a patient";
            s.Responses[200] = "Allergy updated successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient or allergy not found";
        });
    }

    public override async Task HandleAsync(UpdateAllergyRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        patient.UpdateAllergy(req.AllergyId, req.Severity, req.Notes);

        await patientRepository.UpdateAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var updatedAllergy = patient.Allergies.First(a => a.Id == req.AllergyId);

        await Send.OkAsync(new UpdateAllergyResponse
        {
            Id = updatedAllergy.Id,
            PatientId = updatedAllergy.PatientId,
            Name = updatedAllergy.Name,
            Severity = updatedAllergy.Severity,
            Notes = updatedAllergy.Notes,
            UpdatedAt = updatedAllergy.UpdatedAt
        }, ct);
    }
}

