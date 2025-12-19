using Ardalis.GuardClauses;
using MedicalCenter.Core.Abstractions;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.Core.Aggregates.Doctors;

/// <summary>
/// Doctor aggregate root.
/// Doctors can create medical records and view patient data.
/// </summary>
public class Doctor : User, IAggregateRoot
{
    public string LicenseNumber { get; private set; } = string.Empty;
    public string Specialty { get; private set; } = string.Empty;

    private Doctor() { } // EF Core

    public Doctor(Guid id, string fullName, string email, string licenseNumber, string specialty)
        : base(id, fullName, email, UserRole.Doctor)
    {
        LicenseNumber = licenseNumber;
        Specialty = specialty;
    }

    public static Doctor Create(string fullName, string email, string licenseNumber, string specialty)
    {
        Guard.Against.NullOrWhiteSpace(licenseNumber, nameof(licenseNumber));
        Guard.Against.NullOrWhiteSpace(specialty, nameof(specialty));
        
        return new Doctor(Guid.NewGuid(), fullName, email, licenseNumber, specialty);
    }

    public void UpdateSpecialty(string specialty)
    {
        Guard.Against.NullOrWhiteSpace(specialty, nameof(specialty));
        Specialty = specialty;
    }
}

