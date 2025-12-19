using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Authorization;
using MedicalCenter.WebApi.Attributes;

namespace MedicalCenter.WebApi.Endpoints.Patients.Medications;

/// <summary>
/// Create medication for a patient endpoint.
/// </summary>
[ActionLog("Medication created for patient")]
public class CreateMedicationEndpoint(
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<CreateMedicationRequest, CreateMedicationResponse>
{
    public override void Configure()
    {
        Post("/");
        Group<MedicationsGroup>();
        Policies(AuthorizationPolicies.CanModifyMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "Create medication for patient";
            s.Description = "Creates a new medication for a specific patient";
            s.Responses[200] = "Medication created successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(CreateMedicationRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        var medication = patient.AddMedication(req.Name, req.Dosage, req.StartDate, req.EndDate, req.Notes);

        await patientRepository.UpdateAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await Send.OkAsync(new CreateMedicationResponse
        {
            Id = medication.Id,
            PatientId = medication.PatientId,
            Name = medication.Name,
            Dosage = medication.Dosage,
            StartDate = medication.StartDate,
            EndDate = medication.EndDate,
            Notes = medication.Notes,
            CreatedAt = medication.CreatedAt
        }, ct);
    }
}

