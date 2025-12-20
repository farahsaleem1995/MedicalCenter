using FastEndpoints;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Authorization;
using MedicalCenter.Core.Queries;

namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Get current patient's specific encounter endpoint.
/// </summary>
public class GetSelfEncounterEndpoint(
    IEncounterQueryService encounterQueryService,
    IUserContext userContext)
    : Endpoint<GetSelfEncounterRequest, GetSelfEncounterResponse>
{
    public override void Configure()
    {
        Get("/self/encounters/{encounterId}");
        Group<PatientsGroup>();
        Policies(AuthorizationPolicies.RequirePatient);
        Summary(s =>
        {
            s.Summary = "Get patient's specific encounter";
            s.Description = "Returns a specific encounter for the authenticated patient.";
            s.Responses[200] = "Encounter retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - encounter does not belong to patient";
            s.Responses[404] = "Encounter not found";
        });
    }

    public override async Task HandleAsync(GetSelfEncounterRequest req, CancellationToken ct)
    {
        var patientId = userContext.UserId;

        var encounter = await encounterQueryService.GetEncounterByIdAsync(req.EncounterId, ct);

        if (encounter == null || encounter.PatientId != patientId)
        {
            ThrowError("Encounter not found", 404);
            return;
        }

        await Send.OkAsync(new GetSelfEncounterResponse
        {
            Id = encounter.Id,
            Practitioner = new GetSelfEncounterResponse.PractitionerDto
            {
                FullName = encounter.Practitioner.FullName,
                Role = encounter.Practitioner.Role
            },
            OccurredOn = encounter.OccurredOn,
            Reason = encounter.Reason
        }, ct);
    }
}

