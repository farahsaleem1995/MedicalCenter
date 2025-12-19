using MedicalCenter.Core.Aggregates.ActionLogs;
using MedicalCenter.Core.Primitives.Pagination;

namespace MedicalCenter.WebApi.Endpoints.ActionLogs;

/// <summary>
/// Response DTO for action log history.
/// </summary>
public class GetActionLogsResponse
{
    /// <summary>
    /// List of action log entries.
    /// </summary>
    public IReadOnlyList<ActionLogEntryDto> Items { get; set; } = Array.Empty<ActionLogEntryDto>();

    /// <summary>
    /// Pagination metadata.
    /// </summary>
    public PaginationMetadata Metadata { get; set; } = null!;
}

/// <summary>
/// DTO for action log entry.
/// </summary>
public class ActionLogEntryDto
{
    public Guid Id { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? Payload { get; set; }
    public DateTime ExecutedAt { get; set; }

    /// <summary>
    /// Creates a DTO from an ActionLogEntry aggregate.
    /// </summary>
    public static ActionLogEntryDto FromActionLogEntry(ActionLogEntry entry)
    {
        return new ActionLogEntryDto
        {
            Id = entry.Id,
            ActionName = entry.ActionName,
            Description = entry.Description,
            UserId = entry.UserId,
            Payload = entry.Payload,
            ExecutedAt = entry.ExecutedAt
        };
    }
}

