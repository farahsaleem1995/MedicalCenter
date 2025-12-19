using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Authorization;
using MedicalCenter.WebApi.Attributes;

namespace MedicalCenter.WebApi.Endpoints.Patients.Allergies;

/// <summary>
/// Delete allergy endpoint.
/// </summary>
[ActionLog("Patient allergy deleted")]
public class DeleteAllergyEndpoint(
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<DeleteAllergyRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("/{allergyId}");
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

        await Send.NoContentAsync(ct);
    }
}

