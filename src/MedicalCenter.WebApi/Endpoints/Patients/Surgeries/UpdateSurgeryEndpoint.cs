using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients.Surgeries;

/// <summary>
/// Update surgery endpoint.
/// </summary>
public class UpdateSurgeryEndpoint(
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<UpdateSurgeryRequest, UpdateSurgeryResponse>
{
    public override void Configure()
    {
        Put("/{surgeryId}");
        Group<SurgeriesGroup>();
        Policies(AuthorizationPolicies.CanModifyMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "Update surgery";
            s.Description = "Updates an existing surgery for a patient";
            s.Responses[200] = "Surgery updated successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient or surgery not found";
        });
    }

    public override async Task HandleAsync(UpdateSurgeryRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        patient.UpdateSurgery(req.SurgeryId, req.Surgeon, req.Notes);

        await patientRepository.UpdateAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var updatedSurgery = patient.Surgeries.First(s => s.Id == req.SurgeryId);

        await Send.OkAsync(new UpdateSurgeryResponse
        {
            Id = updatedSurgery.Id,
            PatientId = updatedSurgery.PatientId,
            Name = updatedSurgery.Name,
            Date = updatedSurgery.Date,
            Surgeon = updatedSurgery.Surgeon,
            Notes = updatedSurgery.Notes,
            UpdatedAt = updatedSurgery.UpdatedAt
        }, ct);
    }
}

