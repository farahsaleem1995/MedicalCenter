using Ardalis.GuardClauses;
using MedicalCenter.Core.Enums;

namespace MedicalCenter.Core.Entities;

/// <summary>
/// Doctor entity (not an aggregate).
/// Doctors can create medical records and view patient data.
/// </summary>
public class Doctor : User
{
    public string LicenseNumber { get; private set; } = string.Empty;
    public string Specialty { get; private set; } = string.Empty;

    private Doctor() { } // EF Core

    public Doctor(string fullName, string email, string licenseNumber, string specialty)
        : base(fullName, email, UserRole.Doctor)
    {
        LicenseNumber = licenseNumber;
        Specialty = specialty;
    }

    public static Doctor Create(string fullName, string email, string licenseNumber, string specialty)
    {
        Guard.Against.NullOrWhiteSpace(licenseNumber, nameof(licenseNumber));
        Guard.Against.NullOrWhiteSpace(specialty, nameof(specialty));
        
        return new Doctor(fullName, email, licenseNumber, specialty);
    }

    public void UpdateSpecialty(string specialty)
    {
        Guard.Against.NullOrWhiteSpace(specialty, nameof(specialty));
        Specialty = specialty;
    }
}

