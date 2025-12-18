using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients.Medications;

/// <summary>
/// Update medication endpoint.
/// </summary>
public class UpdateMedicationEndpoint(
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<UpdateMedicationRequest, UpdateMedicationResponse>
{
    public override void Configure()
    {
        Put("/{medicationId}");
        Group<MedicationsGroup>();
        Policies(AuthorizationPolicies.CanModifyMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "Update medication";
            s.Description = "Updates an existing medication for a patient";
            s.Responses[200] = "Medication updated successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient or medication not found";
        });
    }

    public override async Task HandleAsync(UpdateMedicationRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        patient.UpdateMedication(req.MedicationId, req.Dosage, req.EndDate, req.Notes);

        await patientRepository.UpdateAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var updatedMedication = patient.Medications.First(m => m.Id == req.MedicationId);

        await Send.OkAsync(new UpdateMedicationResponse
        {
            Id = updatedMedication.Id,
            PatientId = updatedMedication.PatientId,
            Name = updatedMedication.Name,
            Dosage = updatedMedication.Dosage,
            StartDate = updatedMedication.StartDate,
            EndDate = updatedMedication.EndDate,
            Notes = updatedMedication.Notes,
            UpdatedAt = updatedMedication.UpdatedAt
        }, ct);
    }
}

