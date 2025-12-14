using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Aggregates.Patient.Specifications;
using MedicalCenter.Core.Repositories;
using MedicalCenter.Infrastructure.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients.Allergies;

/// <summary>
/// Delete allergy endpoint.
/// </summary>
public class DeleteAllergyEndpoint(
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<DeleteAllergyRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("/patients/{patientId}/allergies/{allergyId}");
        Group<AllergiesGroup>();
        Policies(AuthorizationPolicies.CanModifyMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "Delete allergy";
            s.Description = "Deletes an existing allergy for a patient";
            s.Responses[204] = "Allergy deleted successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient or allergy not found";
        });
    }

    public override async Task HandleAsync(DeleteAllergyRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        if (!patient.Allergies.Any(a => a.Id == req.AllergyId))
        {
            ThrowError("Allergy not found", 404);
            return;
        }

        patient.RemoveAllergy(req.AllergyId);

        await patientRepository.UpdateAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);

        HttpContext.Response.StatusCode = 204;
    }
}

