using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.WebApi.Attributes;
using MedicalCenter.WebApi.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients.Surgeries;

/// <summary>
/// Create surgery for a patient endpoint.
/// </summary>
[ActionLog("Surgery created for patient")]
public class CreateSurgeryEndpoint(
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<CreateSurgeryRequest, CreateSurgeryResponse>
{
    public override void Configure()
    {
        Post("/");
        Group<SurgeriesGroup>();
        Policies(AuthorizationPolicies.CanModifyMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "Create surgery for patient";
            s.Description = "Creates a new surgery for a specific patient";
            s.Responses[200] = "Surgery created successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(CreateSurgeryRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        var surgery = patient.AddSurgery(req.Name, req.Date, req.Surgeon, req.Notes);

        await patientRepository.UpdateAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await Send.OkAsync(new CreateSurgeryResponse
        {
            Id = surgery.Id,
            PatientId = surgery.PatientId,
            Name = surgery.Name,
            Date = surgery.Date,
            Surgeon = surgery.Surgeon,
            Notes = surgery.Notes,
            CreatedAt = surgery.CreatedAt
        }, ct);
    }
}

