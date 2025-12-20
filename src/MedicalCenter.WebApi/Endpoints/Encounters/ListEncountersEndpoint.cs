using FastEndpoints;
using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Core.Queries;
using MedicalCenter.WebApi.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Encounters;

/// <summary>
/// List all encounters endpoint.
/// Practitioners and admins can view all encounters with optional filtering.
/// </summary>
public class ListEncountersEndpoint(IEncounterQueryService encounterQueryService)
    : Endpoint<ListEncountersRequest, ListEncountersResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<EncountersGroup>();
        Policies(AuthorizationPolicies.CanViewEncounters);
        Summary(s =>
        {
            s.Summary = "List encounters";
            s.Description = "Lists all encounters. Practitioners and admins can view all encounters. Supports filtering by patient and date range. Supports pagination.";
            s.Params["pageNumber"] = "Page number (default: 1, minimum: 1)";
            s.Params["pageSize"] = "Number of items per page (default: 10, minimum: 1, maximum: 100)";
            s.Params["patientId"] = "Optional: Filter by patient ID";
            s.Params["dateFrom"] = "Optional: Filter encounters from this date";
            s.Params["dateTo"] = "Optional: Filter encounters to this date";
            s.Responses[200] = "Encounters retrieved successfully";
            s.Responses[401] = "Unauthorized";
        });
    }

    public override async Task HandleAsync(ListEncountersRequest req, CancellationToken ct)
    {
        var query = new PaginationQuery<ListEncountersQuery>(req.PageNumber ?? 1, req.PageSize ?? 10)
        {
            Criteria = new ListEncountersQuery
            {
                PatientId = req.PatientId,
                DateFrom = req.DateFrom,
                DateTo = req.DateTo
            }
        };

        var paginatedResult = await encounterQueryService.ListEncountersAsync(query, ct);

        await Send.OkAsync(new ListEncountersResponse
        {
            Items = paginatedResult.Items.Select(EncounterSummaryDto.FromEncounter).ToList(),
            Metadata = paginatedResult.Metadata
        }, ct);
    }
}

