using Ardalis.GuardClauses;
using MedicalCenter.Core.Abstractions;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.Core.Aggregates.Laboratories;

/// <summary>
/// Laboratory aggregate root.
/// Lab users can create lab records and view related patient data.
/// </summary>
public class Laboratory : User, IAggregateRoot
{
    public string LabName { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;

    private Laboratory() { } // EF Core

    public Laboratory(string fullName, string email, string labName, string licenseNumber)
        : base(fullName, email, UserRole.LabUser)
    {
        LabName = labName;
        LicenseNumber = licenseNumber;
    }

    public static Laboratory Create(string fullName, string email, string labName, string licenseNumber)
    {
        Guard.Against.NullOrWhiteSpace(labName, nameof(labName));
        Guard.Against.NullOrWhiteSpace(licenseNumber, nameof(licenseNumber));
        
        return new Laboratory(fullName, email, labName, licenseNumber);
    }

    public void UpdateLabName(string labName)
    {
        Guard.Against.NullOrWhiteSpace(labName, nameof(labName));
        LabName = labName;
    }
}

