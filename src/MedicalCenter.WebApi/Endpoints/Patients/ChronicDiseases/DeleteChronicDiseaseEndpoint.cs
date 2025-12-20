using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.WebApi.Attributes;
using MedicalCenter.WebApi.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients.ChronicDiseases;

/// <summary>
/// Delete chronic disease endpoint.
/// </summary>
[ActionLog("Patient chronic disease deleted")]
public class DeleteChronicDiseaseEndpoint(
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<DeleteChronicDiseaseRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("/{chronicDiseaseId}");
        Group<ChronicDiseasesGroup>();
        Policies(AuthorizationPolicies.CanModifyMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "Delete chronic disease";
            s.Description = "Deletes an existing chronic disease for a patient";
            s.Responses[204] = "Chronic disease deleted successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient or chronic disease not found";
        });
    }

    public override async Task HandleAsync(DeleteChronicDiseaseRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        if (!patient.ChronicDiseases.Any(cd => cd.Id == req.ChronicDiseaseId))
        {
            ThrowError("Chronic disease not found", 404);
            return;
        }

        patient.RemoveChronicDisease(req.ChronicDiseaseId);

        await patientRepository.UpdateAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}

