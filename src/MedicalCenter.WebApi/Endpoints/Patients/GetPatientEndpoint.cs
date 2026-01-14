using FastEndpoints;
using MedicalCenter.Core.Queries;
using MedicalCenter.WebApi.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Get specific patient endpoint.
/// </summary>
public class GetPatientEndpoint(IPatientQueryService patientQueryService)
    : Endpoint<GetPatientRequest, GetPatientResponse>
{
    public override void Configure()
    {
        Get("/{patientId}");
        Group<PatientsGroup>();
        Policies(AuthorizationPolicies.CanViewPatients);
        Summary(s =>
        {
            s.Summary = "Get patient";
            s.Description = "Gets a specific patient by ID. Practitioners can view all patients.";
            s.Responses[200] = "Patient retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(GetPatientRequest req, CancellationToken ct)
    {
        var patient = await patientQueryService.GetPatientByIdAsync(req.PatientId, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        await Send.OkAsync(GetPatientResponse.FromPatient(patient), ct);
    }
}

