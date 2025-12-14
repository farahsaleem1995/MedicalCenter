using MedicalCenter.Core.Enums;

namespace MedicalCenter.Core.Entities;

/// <summary>
/// Patient aggregate root.
/// Patients can self-register and access their own medical records.
/// </summary>
public class Patient : User, IAggregateRoot
{
    public string NationalId { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }

    private Patient() { } // EF Core

    public Patient(string fullName, string email, string nationalId, DateTime dateOfBirth)
        : base(fullName, email, UserRole.Patient)
    {
        NationalId = nationalId;
        DateOfBirth = dateOfBirth;
    }

    public static Patient Create(string fullName, string email, string nationalId, DateTime dateOfBirth)
    {
        return new Patient(fullName, email, nationalId, dateOfBirth);
    }
}

