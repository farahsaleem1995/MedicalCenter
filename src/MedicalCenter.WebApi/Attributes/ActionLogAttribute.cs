namespace MedicalCenter.WebApi.Attributes;

/// <summary>
/// Attribute to mark endpoints that should have their actions logged.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ActionLogAttribute : Attribute
{
    /// <summary>
    /// Gets the description of the action to be logged.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionLogAttribute"/> class.
    /// </summary>
    /// <param name="description">Descriptive message for the action. Example: "System administrator created a new user account"</param>
    public ActionLogAttribute(string description)
    {
        Description = description;
    }
}

