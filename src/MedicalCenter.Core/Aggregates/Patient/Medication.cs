using Ardalis.GuardClauses;
using MedicalCenter.Core.Common;

namespace MedicalCenter.Core.Aggregates.Patient;

/// <summary>
/// Medication entity within the Patient aggregate.
/// </summary>
public class Medication : BaseEntity, IAuditableEntity
{
    public Guid PatientId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Dosage { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public Patient Patient { get; private set; } = null!;

    private Medication() { } // EF Core

    private Medication(Guid patientId, string name, string? dosage, DateTime startDate, DateTime? endDate, string? notes)
    {
        PatientId = patientId;
        Name = name;
        Dosage = dosage;
        StartDate = startDate;
        EndDate = endDate;
        Notes = notes;
    }

    public static Medication Create(Guid patientId, string name, string? dosage, DateTime startDate, DateTime? endDate = null, string? notes = null)
    {
        Guard.Against.Default(patientId, nameof(patientId));
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        
        if (endDate.HasValue)
        {
            Guard.Against.OutOfRange(endDate.Value, nameof(endDate), startDate, DateTime.MaxValue);
        }

        return new Medication(patientId, name, dosage, startDate, endDate, notes);
    }

    public void Update(string? dosage, DateTime? endDate, string? notes)
    {
        Dosage = dosage;
        
        if (endDate.HasValue)
        {
            Guard.Against.OutOfRange(endDate.Value, nameof(endDate), StartDate, DateTime.MaxValue);
        }
        
        EndDate = endDate;
        Notes = notes;
    }
}

