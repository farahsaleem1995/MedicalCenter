using MedicalCenter.Core.Aggregates.ActionLogs;
using MedicalCenter.Core.Primitives.Pagination;

namespace MedicalCenter.Core.Queries;

public interface IActionLogQueryService
{
    /// <summary>
    /// Retrieves paginated action log history with filtering, ordering, and pagination.
    /// Returns aggregate roots directly (following query service pattern).
    /// </summary>
    /// <param name="query">Query criteria including filters, ordering, and pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of action log entries (aggregate roots)</returns>
    public Task<PaginatedList<ActionLogEntry>> GetHistory(
        PaginationQuery<ActionLogQuery> query,
        CancellationToken cancellationToken = default);
}

public class ActionLogQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? UserId { get; set; }
    public string? ActionName { get; set; }
}

