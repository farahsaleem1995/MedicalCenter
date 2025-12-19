namespace MedicalCenter.WebApi.Attributes;

/// <summary>
/// Attribute to mark endpoints as Commands (state-changing operations).
/// Commands are transactional by default and can be audited.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
    /// <summary>
    /// Gets or sets whether this command should be wrapped in a transaction.
    /// Default: true
    /// </summary>
    public bool IsTransactional { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this command should be audited (tracked in audit trail).
    /// Default: true
    /// </summary>
    public bool IsTraceable { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandAttribute"/> class.
    /// </summary>
    public CommandAttribute() { }
}

