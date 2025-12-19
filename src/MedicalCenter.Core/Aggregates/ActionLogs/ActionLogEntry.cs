using MedicalCenter.Core.Abstractions;

namespace MedicalCenter.Core.Aggregates.ActionLogs;

/// <summary>
/// Represents an action log entry - a record of a business-critical action performed in the system.
/// This is an aggregate root with its own consistency boundary.
/// NOT an auditable entity (no CreatedAt/UpdatedAt tracking).
/// </summary>
public class ActionLogEntry : IAggregateRoot
{
    public Guid Id { get; private set; }
    
    /// <summary>
    /// Name of the action (e.g., "CreateUser", "ViewRecord", "DownloadAttachment")
    /// </summary>
    public string ActionName { get; private set; } = string.Empty;
    
    /// <summary>
    /// Descriptive message explaining what action was performed.
    /// Example: "User 'John Doe' created a new medical record for patient 'Jane Smith'"
    /// </summary>
    public string Description { get; private set; } = string.Empty;
    
    /// <summary>
    /// ID of the user who performed the action. Null for anonymous actions.
    /// </summary>
    public Guid? UserId { get; private set; }
    
    /// <summary>
    /// Serialized request payload (with sensitive data filtered).
    /// Max 10KB, can be null for actions without payload.
    /// </summary>
    public string? Payload { get; private set; }
    
    /// <summary>
    /// Timestamp when the action was executed (UTC).
    /// </summary>
    public DateTime ExecutedAt { get; private set; }
    
    // Private constructor for EF Core
    private ActionLogEntry() { }
    
    /// <summary>
    /// Factory method for creating new action log entries.
    /// </summary>
    public static ActionLogEntry Create(
        string actionName,
        string description,
        Guid? userId,
        string? payload)
    {
        return new ActionLogEntry
        {
            Id = Guid.NewGuid(),
            ActionName = actionName,
            Description = description,
            UserId = userId,
            Payload = payload,
            ExecutedAt = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Factory method for reconstructing action log entries from persistence (used by query operations).
    /// </summary>
    internal static ActionLogEntry FromPersistence(
        Guid id,
        string actionName,
        string description,
        Guid? userId,
        string? payload,
        DateTime executedAt)
    {
        return new ActionLogEntry
        {
            Id = id,
            ActionName = actionName,
            Description = description,
            UserId = userId,
            Payload = payload,
            ExecutedAt = executedAt
        };
    }
}

