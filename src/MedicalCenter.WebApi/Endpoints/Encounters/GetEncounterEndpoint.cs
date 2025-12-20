using FastEndpoints;
using MedicalCenter.Core.Queries;
using MedicalCenter.WebApi.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Encounters;

/// <summary>
/// Get specific encounter endpoint.
/// Practitioners and admins can view all encounters.
/// </summary>
public class GetEncounterEndpoint(IEncounterQueryService encounterQueryService)
    : Endpoint<GetEncounterRequest, GetEncounterResponse>
{
    public override void Configure()
    {
        Get("/{encounterId}");
        Group<EncountersGroup>();
        Policies(AuthorizationPolicies.CanViewEncounters);
        Summary(s =>
        {
            s.Summary = "Get encounter";
            s.Description = "Gets a specific encounter. Practitioners and admins can view all encounters.";
            s.Responses[200] = "Encounter retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[404] = "Encounter not found";
        });
    }

    public override async Task HandleAsync(GetEncounterRequest req, CancellationToken ct)
    {
        var encounter = await encounterQueryService.GetEncounterByIdAsync(req.EncounterId, ct);

        if (encounter == null)
        {
            ThrowError("Encounter not found", 404);
            return;
        }

        await Send.OkAsync(new GetEncounterResponse
        {
            Id = encounter.Id,
            PatientId = encounter.PatientId,
            Patient = encounter.Patient != null ? new GetEncounterResponse.PatientSummaryDto
            {
                Id = encounter.Patient.Id,
                FullName = encounter.Patient.FullName,
                Email = encounter.Patient.Email
            } : null,
            Practitioner = new GetEncounterResponse.PractitionerDto
            {
                FullName = encounter.Practitioner.FullName,
                Role = encounter.Practitioner.Role
            },
            OccurredOn = encounter.OccurredOn,
            Reason = encounter.Reason
        }, ct);
    }
}

