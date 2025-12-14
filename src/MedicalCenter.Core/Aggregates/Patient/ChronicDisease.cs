using Ardalis.GuardClauses;
using MedicalCenter.Core.Common;
using MedicalCenter.Core.Entities;

namespace MedicalCenter.Core.Aggregates.Patient;

/// <summary>
/// Chronic disease entity within the Patient aggregate.
/// </summary>
public class ChronicDisease : BaseEntity, IAuditableEntity
{
    public Guid PatientId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime DiagnosisDate { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public Patient Patient { get; private set; } = null!;

    private ChronicDisease() { } // EF Core

    private ChronicDisease(Guid patientId, string name, DateTime diagnosisDate, string? notes)
    {
        PatientId = patientId;
        Name = name;
        DiagnosisDate = diagnosisDate;
        Notes = notes;
    }

    public static ChronicDisease Create(Guid patientId, string name, DateTime diagnosisDate, string? notes = null)
    {
        Guard.Against.Default(patientId, nameof(patientId));
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.OutOfRange(diagnosisDate, nameof(diagnosisDate), DateTime.MinValue, DateTime.UtcNow);

        return new ChronicDisease(patientId, name, diagnosisDate, notes);
    }

    public void Update(string? notes)
    {
        Notes = notes;
    }
}

