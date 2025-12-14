using MedicalCenter.Core.Enums;

namespace MedicalCenter.Core.Entities;

/// <summary>
/// Imaging center entity (not an aggregate).
/// Imaging users can create imaging records and view related patient data.
/// </summary>
public class ImagingCenter : User
{
    public string CenterName { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;

    private ImagingCenter() { } // EF Core

    public ImagingCenter(string fullName, string email, string centerName, string licenseNumber)
        : base(fullName, email, UserRole.ImagingUser)
    {
        CenterName = centerName;
        LicenseNumber = licenseNumber;
    }

    public static ImagingCenter Create(string fullName, string email, string centerName, string licenseNumber)
    {
        return new ImagingCenter(fullName, email, centerName, licenseNumber);
    }
}

