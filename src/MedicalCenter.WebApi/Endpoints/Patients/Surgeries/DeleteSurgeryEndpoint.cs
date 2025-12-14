using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Aggregates.Patient.Specifications;
using MedicalCenter.Core.Repositories;
using MedicalCenter.Infrastructure.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients.Surgeries;

/// <summary>
/// Delete surgery endpoint.
/// </summary>
public class DeleteSurgeryEndpoint(
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<DeleteSurgeryRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("/patients/{patientId}/surgeries/{surgeryId}");
        Group<SurgeriesGroup>();
        Policies(AuthorizationPolicies.CanModifyMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "Delete surgery";
            s.Description = "Deletes an existing surgery for a patient";
            s.Responses[204] = "Surgery deleted successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient or surgery not found";
        });
    }

    public override async Task HandleAsync(DeleteSurgeryRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        if (!patient.Surgeries.Any(s => s.Id == req.SurgeryId))
        {
            ThrowError("Surgery not found", 404);
            return;
        }

        patient.RemoveSurgery(req.SurgeryId);

        await patientRepository.UpdateAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}

