using FastEndpoints;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Authorization;
using MedicalCenter.Core.Queries;
using MedicalCenter.Core.Primitives.Pagination;

namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Get current patient's own encounters endpoint.
/// </summary>
public class ListSelfEncountersEndpoint(
    IEncounterQueryService encounterQueryService,
    IUserContext userContext)
    : Endpoint<ListSelfEncountersRequest, ListSelfEncountersResponse>
{
    public override void Configure()
    {
        Get("/self/encounters");
        Group<PatientsGroup>();
        Policies(AuthorizationPolicies.RequirePatient);
        Summary(s =>
        {
            s.Summary = "Get patient's own encounters";
            s.Description = "Returns all encounters for the authenticated patient. Supports filtering by date range.";
            s.Responses[200] = "Encounters retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(ListSelfEncountersRequest req, CancellationToken ct)
    {
        var patientId = userContext.UserId;

        var encounters = await encounterQueryService.ListEncountersAsync(
            new PaginationQuery<ListEncountersQuery>(req.PageNumber ?? 1, req.PageSize ?? 10)
            {
                Criteria = new ListEncountersQuery
                {
                    PatientId = patientId,
                    DateFrom = req.DateFrom,
                    DateTo = req.DateTo
                }
            }, ct);

        await Send.OkAsync(new ListSelfEncountersResponse
        {
            Encounters = [.. encounters.Items.Select(PatientEncounterSummaryDto.FromEncounter)],
            Metadata = encounters.Metadata
        }, ct);
    }
}

