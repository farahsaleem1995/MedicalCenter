using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.WebApi.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients.Medications;

/// <summary>
/// List medications for a patient endpoint.
/// </summary>
public class ListMedicationsEndpoint(IRepository<Patient> patientRepository)
    : Endpoint<ListMedicationsRequest, ListMedicationsResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<MedicationsGroup>();
        Policies(AuthorizationPolicies.CanViewMedicalAttributes);
        Summary(s =>
        {
            s.Summary = "List patient medications";
            s.Description = "Returns all medications for a specific patient";
            s.Responses[200] = "Medications retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(ListMedicationsRequest req, CancellationToken ct)
    {
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        await Send.OkAsync(new ListMedicationsResponse
        {
            Medications = patient.Medications.Select(m => new MedicationDto
            {
                Id = m.Id,
                PatientId = m.PatientId,
                Name = m.Name,
                Dosage = m.Dosage,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                Notes = m.Notes,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            }).ToList()
        }, ct);
    }
}

