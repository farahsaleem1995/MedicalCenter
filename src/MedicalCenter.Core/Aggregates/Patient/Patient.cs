using Ardalis.GuardClauses;
using MedicalCenter.Core.Common;
using MedicalCenter.Core.Entities;
using MedicalCenter.Core.Enums;
using MedicalCenter.Core.ValueObjects;

namespace MedicalCenter.Core.Aggregates.Patient;

/// <summary>
/// Patient aggregate root.
/// Patients can self-register and access their own medical records.
/// </summary>
public class Patient : User, IAggregateRoot
{
    public string NationalId { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public BloodType? BloodType { get; private set; }

    // Medical attributes collections
    private readonly List<Allergy> _allergies = new();
    private readonly List<ChronicDisease> _chronicDiseases = new();
    private readonly List<Medication> _medications = new();
    private readonly List<Surgery> _surgeries = new();

    public IReadOnlyCollection<Allergy> Allergies => _allergies.AsReadOnly();
    public IReadOnlyCollection<ChronicDisease> ChronicDiseases => _chronicDiseases.AsReadOnly();
    public IReadOnlyCollection<Medication> Medications => _medications.AsReadOnly();
    public IReadOnlyCollection<Surgery> Surgeries => _surgeries.AsReadOnly();

    private Patient() { } // EF Core

    public Patient(string fullName, string email, string nationalId, DateTime dateOfBirth)
        : base(fullName, email, UserRole.Patient)
    {
        NationalId = nationalId;
        DateOfBirth = dateOfBirth;
    }

    public static Patient Create(string fullName, string email, string nationalId, DateTime dateOfBirth)
    {
        Guard.Against.NullOrWhiteSpace(fullName, nameof(fullName));
        Guard.Against.NullOrWhiteSpace(email, nameof(email));
        Guard.Against.NullOrWhiteSpace(nationalId, nameof(nationalId));
        Guard.Against.OutOfRange(dateOfBirth, nameof(dateOfBirth), DateTime.MinValue, DateTime.UtcNow);

        return new Patient(fullName, email, nationalId, dateOfBirth);
    }

    // Blood Type Management
    public void UpdateBloodType(BloodType? bloodType)
    {
        BloodType = bloodType;
    }

    // Allergy Management
    public Allergy AddAllergy(string name, string? severity = null, string? notes = null)
    {
        Guard.Against.Default(Id, nameof(Id));
        var allergy = Allergy.Create(Id, name, severity, notes);
        _allergies.Add(allergy);
        return allergy;
    }

    public void RemoveAllergy(Guid allergyId)
    {
        var allergy = _allergies.FirstOrDefault(a => a.Id == allergyId);
        if (allergy != null)
        {
            _allergies.Remove(allergy);
        }
    }

    public void UpdateAllergy(Guid allergyId, string? severity, string? notes)
    {
        Guard.Against.Default(allergyId, nameof(allergyId));
        
        var allergy = _allergies.FirstOrDefault(a => a.Id == allergyId);
        if (allergy == null)
        {
            throw new InvalidOperationException($"Allergy with ID {allergyId} not found.");
        }

        allergy.Update(severity, notes);
    }

    // Chronic Disease Management
    public ChronicDisease AddChronicDisease(string name, DateTime diagnosisDate, string? notes = null)
    {
        Guard.Against.Default(Id, nameof(Id));
        var chronicDisease = ChronicDisease.Create(Id, name, diagnosisDate, notes);
        _chronicDiseases.Add(chronicDisease);
        return chronicDisease;
    }

    public void RemoveChronicDisease(Guid chronicDiseaseId)
    {
        var chronicDisease = _chronicDiseases.FirstOrDefault(cd => cd.Id == chronicDiseaseId);
        if (chronicDisease != null)
        {
            _chronicDiseases.Remove(chronicDisease);
        }
    }

    public void UpdateChronicDisease(Guid chronicDiseaseId, string? notes)
    {
        Guard.Against.Default(chronicDiseaseId, nameof(chronicDiseaseId));
        
        var chronicDisease = _chronicDiseases.FirstOrDefault(cd => cd.Id == chronicDiseaseId);
        if (chronicDisease == null)
        {
            throw new InvalidOperationException($"Chronic disease with ID {chronicDiseaseId} not found.");
        }

        chronicDisease.Update(notes);
    }

    // Medication Management
    public Medication AddMedication(string name, string? dosage, DateTime startDate, DateTime? endDate = null, string? notes = null)
    {
        Guard.Against.Default(Id, nameof(Id));
        var medication = Medication.Create(Id, name, dosage, startDate, endDate, notes);
        _medications.Add(medication);
        return medication;
    }

    public void RemoveMedication(Guid medicationId)
    {
        var medication = _medications.FirstOrDefault(m => m.Id == medicationId);
        if (medication != null)
        {
            _medications.Remove(medication);
        }
    }

    public void UpdateMedication(Guid medicationId, string? dosage, DateTime? endDate, string? notes)
    {
        Guard.Against.Default(medicationId, nameof(medicationId));
        
        var medication = _medications.FirstOrDefault(m => m.Id == medicationId);
        if (medication == null)
        {
            throw new InvalidOperationException($"Medication with ID {medicationId} not found.");
        }

        medication.Update(dosage, endDate, notes);
    }

    // Surgery Management
    public Surgery AddSurgery(string name, DateTime date, string? surgeon = null, string? notes = null)
    {
        Guard.Against.Default(Id, nameof(Id));
        var surgery = Surgery.Create(Id, name, date, surgeon, notes);
        _surgeries.Add(surgery);
        return surgery;
    }

    public void RemoveSurgery(Guid surgeryId)
    {
        var surgery = _surgeries.FirstOrDefault(s => s.Id == surgeryId);
        if (surgery != null)
        {
            _surgeries.Remove(surgery);
        }
    }

    public void UpdateSurgery(Guid surgeryId, string? surgeon, string? notes)
    {
        Guard.Against.Default(surgeryId, nameof(surgeryId));
        
        var surgery = _surgeries.FirstOrDefault(s => s.Id == surgeryId);
        if (surgery == null)
        {
            throw new InvalidOperationException($"Surgery with ID {surgeryId} not found.");
        }

        surgery.Update(surgeon, notes);
    }
}

