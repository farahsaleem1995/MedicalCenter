using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.Aggregates.Patients.ValueObjects;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Authorization;
using MedicalCenter.WebApi.Attributes;

namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Update patient blood type endpoint.
/// </summary>
[ActionLog("Patient blood type updated")]
public class UpdateBloodTypeEndpoint(
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<UpdateBloodTypeRequest, UpdateBloodTypeResponse>
{
    public override void Configure()
    {
        Put("/{patientId}/blood-type");
        Group<PatientsGroup>();
        Policies(AuthorizationPolicies.CanModifyMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "Update patient blood type";
            s.Description = "Updates the blood type for a specific patient";
            s.Responses[200] = "Blood type updated successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(UpdateBloodTypeRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        BloodType? bloodType = null;
        if (req.ABO.HasValue && req.Rh.HasValue)
        {
            bloodType = BloodType.Create(req.ABO.Value, req.Rh.Value);
        }

        patient.UpdateBloodType(bloodType);

        await patientRepository.UpdateAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await Send.OkAsync(new UpdateBloodTypeResponse
        {
            PatientId = patient.Id,
            BloodType = patient.BloodType?.ToString()
        }, ct);
    }
}
