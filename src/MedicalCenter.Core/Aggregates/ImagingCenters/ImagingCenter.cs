using Ardalis.GuardClauses;
using MedicalCenter.Core.Abstractions;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.Core.Aggregates.ImagingCenters;

/// <summary>
/// Imaging center aggregate root.
/// Imaging users can create imaging records and view related patient data.
/// </summary>
public class ImagingCenter : User, IAggregateRoot
{
    public string CenterName { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;

    private ImagingCenter() { } // EF Core

    public ImagingCenter(Guid id, string fullName, string email, string centerName, string licenseNumber)
        : base(id, fullName, email, UserRole.ImagingUser)
    {
        CenterName = centerName;
        LicenseNumber = licenseNumber;
    }

    public static ImagingCenter Create(string fullName, string email, string centerName, string licenseNumber)
    {
        Guard.Against.NullOrWhiteSpace(centerName, nameof(centerName));
        Guard.Against.NullOrWhiteSpace(licenseNumber, nameof(licenseNumber));
        
        return new ImagingCenter(Guid.NewGuid(), fullName, email, centerName, licenseNumber);
    }

    public void UpdateCenterName(string centerName)
    {
        Guard.Against.NullOrWhiteSpace(centerName, nameof(centerName));
        CenterName = centerName;
    }
}

