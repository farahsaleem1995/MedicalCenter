namespace MedicalCenter.WebApi.Attributes;

/// <summary>
/// Attribute to mark endpoints as Queries (read-only operations).
/// Queries are never transactional and are monitored for performance.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class QueryAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryAttribute"/> class.
    /// </summary>
    public QueryAttribute() { }
}

