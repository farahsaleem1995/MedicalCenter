namespace MedicalCenter.Core.SharedKernel.Events;

/// <summary>
/// Interface for dispatching domain events.
/// The event dispatcher is responsible for finding and invoking all registered handlers for a domain event.
/// </summary>
public interface IEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

