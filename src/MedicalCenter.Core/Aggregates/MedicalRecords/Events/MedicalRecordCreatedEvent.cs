using MedicalCenter.Core.SharedKernel.Events;

namespace MedicalCenter.Core.Aggregates.MedicalRecords.Events;

/// <summary>
/// Domain event raised when a medical record is created.
/// Triggers automatic Encounter creation.
/// </summary>
public class MedicalRecordCreatedEvent(MedicalRecord record) : DomainEventBase
{
    public MedicalRecord Record { get; } = record;
}

