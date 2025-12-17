using Ardalis.GuardClauses;
using MedicalCenter.Core.Common;

namespace MedicalCenter.Core.Aggregates.Patient;

/// <summary>
/// Surgery entity within the Patient aggregate.
/// </summary>
public class Surgery : BaseEntity, IAuditableEntity
{
    public Guid PatientId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public string? Surgeon { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public Patient Patient { get; private set; } = null!;

    private Surgery() { } // EF Core

    private Surgery(Guid patientId, string name, DateTime date, string? surgeon, string? notes)
    {
        PatientId = patientId;
        Name = name;
        Date = date;
        Surgeon = surgeon;
        Notes = notes;
    }

    public static Surgery Create(Guid patientId, string name, DateTime date, string? surgeon = null, string? notes = null)
    {
        Guard.Against.Default(patientId, nameof(patientId));
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.OutOfRange(date, nameof(date), DateTime.MinValue, DateTime.UtcNow);

        return new Surgery(patientId, name, date, surgeon, notes);
    }

    public void Update(string? surgeon, string? notes)
    {
        Surgeon = surgeon;
        Notes = notes;
    }
}

