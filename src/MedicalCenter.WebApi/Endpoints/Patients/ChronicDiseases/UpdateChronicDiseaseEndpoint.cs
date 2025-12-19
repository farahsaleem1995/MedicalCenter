using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Authorization;
using MedicalCenter.WebApi.Attributes;

namespace MedicalCenter.WebApi.Endpoints.Patients.ChronicDiseases;

/// <summary>
/// Update chronic disease endpoint.
/// </summary>
[ActionLog("Patient chronic disease updated")]
public class UpdateChronicDiseaseEndpoint(
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<UpdateChronicDiseaseRequest, UpdateChronicDiseaseResponse>
{
    public override void Configure()
    {
        Put("/{chronicDiseaseId}");
        Group<ChronicDiseasesGroup>();
        Policies(AuthorizationPolicies.CanModifyMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "Update chronic disease";
            s.Description = "Updates an existing chronic disease for a patient";
            s.Responses[200] = "Chronic disease updated successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient or chronic disease not found";
        });
    }

    public override async Task HandleAsync(UpdateChronicDiseaseRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        patient.UpdateChronicDisease(req.ChronicDiseaseId, req.Notes);

        await patientRepository.UpdateAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var updatedChronicDisease = patient.ChronicDiseases.First(cd => cd.Id == req.ChronicDiseaseId);

        await Send.OkAsync(new UpdateChronicDiseaseResponse
        {
            Id = updatedChronicDisease.Id,
            PatientId = updatedChronicDisease.PatientId,
            Name = updatedChronicDisease.Name,
            DiagnosisDate = updatedChronicDisease.DiagnosisDate,
            Notes = updatedChronicDisease.Notes,
            UpdatedAt = updatedChronicDisease.UpdatedAt
        }, ct);
    }
}

