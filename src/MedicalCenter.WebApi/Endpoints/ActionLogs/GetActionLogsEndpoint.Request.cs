namespace MedicalCenter.WebApi.Endpoints.ActionLogs;

/// <summary>
/// Request DTO for getting action log history.
/// </summary>
public class GetActionLogsRequest
{
    /// <summary>
    /// Page number (1-based). Default: 1
    /// </summary>
    public int? PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page. Default: 20
    /// </summary>
    public int? PageSize { get; set; } = 20;

    /// <summary>
    /// Optional start date filter (inclusive). Format: ISO 8601 (e.g., "2024-01-01T00:00:00Z")
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Optional end date filter (inclusive). Format: ISO 8601 (e.g., "2024-12-31T23:59:59Z")
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Optional user ID filter. Returns actions from a specific user.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Optional action name filter. Returns actions matching the specified action name.
    /// </summary>
    public string? ActionName { get; set; }
}

