using Ardalis.GuardClauses;
using MedicalCenter.Core.Common;
using MedicalCenter.Core.Entities;

namespace MedicalCenter.Core.Aggregates.Patient;

/// <summary>
/// Allergy entity within the Patient aggregate.
/// </summary>
public class Allergy : BaseEntity, IAuditableEntity
{
    public Guid PatientId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Severity { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public Patient Patient { get; private set; } = null!;

    private Allergy() { } // EF Core

    private Allergy(Guid patientId, string name, string? severity, string? notes)
    {
        PatientId = patientId;
        Name = name;
        Severity = severity;
        Notes = notes;
    }

    public static Allergy Create(Guid patientId, string name, string? severity = null, string? notes = null)
    {
        Guard.Against.Default(patientId, nameof(patientId));
        Guard.Against.NullOrWhiteSpace(name, nameof(name));

        return new Allergy(patientId, name, severity, notes);
    }

    public void Update(string? severity, string? notes)
    {
        Severity = severity;
        Notes = notes;
    }
}

