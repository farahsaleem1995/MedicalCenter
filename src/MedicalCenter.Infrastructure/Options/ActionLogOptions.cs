namespace MedicalCenter.Infrastructure.Options;

/// <summary>
/// Configuration options for action log service.
/// </summary>
public class ActionLogOptions
{
    public const string SectionName = "ActionLog";
    
    /// <summary>
    /// Capacity of the action log queue.
    /// Default: 1000
    /// </summary>
    public int QueueCapacity { get; set; } = 1000;
}

