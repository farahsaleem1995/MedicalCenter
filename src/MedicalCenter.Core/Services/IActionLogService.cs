using MedicalCenter.Core.Aggregates.ActionLogs;
using MedicalCenter.Core.Primitives.Pagination;

namespace MedicalCenter.Core.Services;

/// <summary>
/// Service for recording and querying action log entries.
/// ActionLog is an aggregate with its own consistency boundary.
/// </summary>
public interface IActionLogService
{
    /// <summary>
    /// Records an action log entry (synchronous, non-blocking).
    /// Enqueues entry for background processing - similar to logger pattern.
    /// </summary>
    /// <param name="entry">The action log entry to record</param>
    public void Record(ActionLogEntry entry);
    
    /// <summary>
    /// Retrieves paginated action log history with filtering, ordering, and pagination.
    /// Returns aggregate roots directly (following query service pattern).
    /// </summary>
    /// <param name="query">Query criteria including filters, ordering, and pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of action log entries (aggregate roots)</returns>
    public Task<PaginatedList<ActionLogEntry>> GetHistory(
        ActionLogQuery query,
        CancellationToken cancellationToken = default);
}

