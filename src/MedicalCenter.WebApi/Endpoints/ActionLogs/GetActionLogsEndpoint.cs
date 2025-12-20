using FastEndpoints;
using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Core.Queries;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Authorization;

namespace MedicalCenter.WebApi.Endpoints.ActionLogs;

/// <summary>
/// Endpoint to retrieve action log history with filtering and pagination.
/// </summary>
public class GetActionLogsEndpoint(
    IActionLogQueryService actionLogQueryService)
    : Endpoint<GetActionLogsRequest, GetActionLogsResponse>
{
    public override void Configure()
    {
        Get("/action-logs");
        Group<ActionLogsGroup>();
        Policies(AuthorizationPolicies.CanViewActionLog);
        Summary(s =>
        {
            s.Summary = "Get action log history";
            s.Description = "Retrieves paginated action log history with optional filtering by date range, user, and action name. Only accessible to system administrators.";
            s.Params["pageNumber"] = "Page number (default: 1, minimum: 1)";
            s.Params["pageSize"] = "Number of items per page (default: 20, minimum: 1, maximum: 100)";
            s.Params["startDate"] = "Optional: Start date filter (ISO 8601 format)";
            s.Params["endDate"] = "Optional: End date filter (ISO 8601 format)";
            s.Params["userId"] = "Optional: Filter by user ID";
            s.Params["actionName"] = "Optional: Filter by action name";
            s.Responses[200] = "Action log history retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - Admin access required";
        });
    }

    public override async Task HandleAsync(GetActionLogsRequest req, CancellationToken ct)
    {
        // Map request to query
        var query = new PaginationQuery<ActionLogQuery>(req.PageNumber ?? 1, req.PageSize ?? 20)
        {
            Criteria = new ActionLogQuery
            {
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                UserId = req.UserId,
                ActionName = req.ActionName
            }
        };

        // Get paginated results
        var result = await actionLogQueryService.GetHistory(query, ct);

        // Map to response DTO
        GetActionLogsResponse response = new GetActionLogsResponse
        {
            Items = result.Items.Select(ActionLogEntryDto.FromActionLogEntry).ToList(),
            Metadata = result.Metadata
        };

        await Send.OkAsync(response, ct);
    }
}

