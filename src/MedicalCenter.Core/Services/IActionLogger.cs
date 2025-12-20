using MedicalCenter.Core.Aggregates.ActionLogs;
using MedicalCenter.Core.Primitives.Pagination;

namespace MedicalCenter.Core.Services;

/// <summary>
/// Service for recording and querying action log entries.
/// ActionLog is an aggregate with its own consistency boundary.
/// </summary>
public interface IActionLogger
{
    /// <summary>
    /// Records an action log entry (synchronous, non-blocking).
    /// Enqueues entry for background processing - similar to logger pattern.
    /// </summary>
    /// <param name="entry">The action log entry to record</param>
    public void Record(ActionLogEntry entry);
}
    