namespace MedicalCenter.Core.SharedKernel.Events;

/// <summary>
/// Interface for handling domain events.
/// Domain event handlers process domain events after they are raised.
/// </summary>
/// <typeparam name="T">The type of domain event to handle.</typeparam>
public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    Task HandleAsync(T domainEvent, CancellationToken cancellationToken = default);
}

