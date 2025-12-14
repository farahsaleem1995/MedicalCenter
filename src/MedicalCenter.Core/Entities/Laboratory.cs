using MedicalCenter.Core.Enums;

namespace MedicalCenter.Core.Entities;

/// <summary>
/// Laboratory entity (not an aggregate).
/// Lab users can create lab records and view related patient data.
/// </summary>
public class Laboratory : User
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
        return new Laboratory(fullName, email, labName, licenseNumber);
    }
}

