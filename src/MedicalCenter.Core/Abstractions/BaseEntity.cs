namespace MedicalCenter.Core.Abstractions;

/// <summary>
/// Base class for all domain entities.
/// Contains only the Id property - audit properties are handled via IAuditableEntity interface.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }

    protected BaseEntity(Guid id)
    {
        Id = id;
    }
}

