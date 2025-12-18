namespace MedicalCenter.Core.SharedKernel.Events;

/// <summary>
/// Marker interface for all domain events.
/// Domain events represent something that happened in the domain that domain experts care about.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

