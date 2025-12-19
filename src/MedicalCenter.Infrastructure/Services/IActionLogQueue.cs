using MedicalCenter.Core.Aggregates.ActionLogs;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Queue service for action log entries using BoundedChannel.
/// Provides thread-safe, bounded queue for action log processing.
/// </summary>
public interface IActionLogQueue
{
    /// <summary>
    /// Enqueues an action log entry for background processing.
    /// Non-blocking operation (returns false if queue is full).
    /// </summary>
    bool TryEnqueue(ActionLogEntry entry);
    
    /// <summary>
    /// Asynchronously reads an action log entry from the queue.
    /// </summary>
    ValueTask<ActionLogEntry?> DequeueAsync(CancellationToken cancellationToken = default);
}

