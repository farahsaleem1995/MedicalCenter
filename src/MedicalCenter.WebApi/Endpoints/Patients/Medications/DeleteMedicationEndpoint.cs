using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Authorization;
using MedicalCenter.WebApi.Attributes;

namespace MedicalCenter.WebApi.Endpoints.Patients.Medications;

/// <summary>
/// Delete medication endpoint.
/// </summary>
[Command]
public class DeleteMedicationEndpoint(
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<DeleteMedicationRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("/{medicationId}");
        Group<MedicationsGroup>();
        Policies(AuthorizationPolicies.CanModifyMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "Delete medication";
            s.Description = "Deletes an existing medication for a patient";
            s.Responses[204] = "Medication deleted successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient or medication not found";
        });
    }

    public override async Task HandleAsync(DeleteMedicationRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        if (!patient.Medications.Any(m => m.Id == req.MedicationId))
        {
            ThrowError("Medication not found", 404);
            return;
        }

        patient.RemoveMedication(req.MedicationId);

        await patientRepository.UpdateAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}

