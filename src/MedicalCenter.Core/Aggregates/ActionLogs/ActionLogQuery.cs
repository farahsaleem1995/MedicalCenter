namespace MedicalCenter.Core.Aggregates.ActionLogs;

/// <summary>
/// Query criteria for retrieving action log history with filtering, ordering, and pagination.
/// </summary>
public class ActionLogQuery
{
    /// <summary>
    /// Page number (1-based).
    /// </summary>
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// Optional start date filter (inclusive). If null, no start date filter.
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Optional end date filter (inclusive). If null, no end date filter.
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Optional user ID filter. If null, returns actions from all users.
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Optional action name filter. If null or empty, returns all action types.
    /// </summary>
    public string? ActionName { get; set; }
}

