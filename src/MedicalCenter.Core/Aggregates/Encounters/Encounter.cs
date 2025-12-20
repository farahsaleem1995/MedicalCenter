using Ardalis.GuardClauses;
using MedicalCenter.Core.Abstractions;
using MedicalCenter.Core.Aggregates.Encounters.ValueObjects;
using MedicalCenter.Core.Aggregates.Patients;

namespace MedicalCenter.Core.Aggregates.Encounters;

/// <summary>
/// Encounter aggregate root.
/// Represents a clinically meaningful interaction between a patient and the healthcare system.
/// A medical event with clinical relevance that contributes to the patient's medical history.
/// Encounters are immutable historical facts - once created, they cannot be modified.
/// NOT auditable - only has OccurredOn (when the medical event occurred).
/// </summary>
public class Encounter : BaseEntity, IAggregateRoot
{
    public Guid PatientId { get; private set; }
    public Patient? Patient { get; private set; } // Navigation property
    public Practitioner Practitioner { get; private set; } = null!; // Practitioner value object (owned entity)
    public DateTime OccurredOn { get; private set; } // When the medical event occurred (NOT CreatedAt)
    public string Reason { get; private set; } = string.Empty; // What happened medically

    private Encounter() { } // EF Core

    private Encounter(Guid patientId, Practitioner practitioner, DateTime occurredOn, string reason)
    {
        PatientId = patientId;
        Practitioner = practitioner;
        OccurredOn = occurredOn;
        Reason = reason;
    }

    public static Encounter Create(Guid patientId, Practitioner practitioner, DateTime occurredOn, string reason)
    {
        Guard.Against.Default(patientId, nameof(patientId));
        Guard.Against.Null(practitioner, nameof(practitioner));
        Guard.Against.NullOrWhiteSpace(reason, nameof(reason));

        return new Encounter(patientId, practitioner, occurredOn, reason);
    }
}

