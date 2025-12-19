using MedicalCenter.Core.SharedKernel.Events;
using System.Collections.ObjectModel;

namespace MedicalCenter.Core.Abstractions;

/// <summary>
/// Base class for all domain entities.
/// Contains only the Id property - audit properties are handled via IAuditableEntity interface.
/// Implements IHasDomainEvents to support domain event raising.
/// </summary>
public abstract class BaseEntity : IHasDomainEvents
{
    public Guid Id { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => new ReadOnlyCollection<IDomainEvent>(_domainEvents);

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }

    protected BaseEntity(Guid id)
    {
        Id = id;
    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

