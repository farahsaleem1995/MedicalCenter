namespace MedicalCenter.Core.SharedKernel.Events;

/// <summary>
/// Interface for entities that can raise domain events.
/// Entities implementing this interface maintain a collection of domain events
/// that are dispatched after the unit of work completes successfully.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    
    void AddDomainEvent(IDomainEvent domainEvent);
    
    void RemoveDomainEvent(IDomainEvent domainEvent);
    
    void ClearDomainEvents();
}

