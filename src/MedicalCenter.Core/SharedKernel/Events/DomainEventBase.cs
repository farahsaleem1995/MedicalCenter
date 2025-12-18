namespace MedicalCenter.Core.SharedKernel.Events;

/// <summary>
/// Base class for all domain events.
/// Provides a default implementation of IDomainEvent with OccurredOn timestamp.
/// </summary>
public abstract class DomainEventBase : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

