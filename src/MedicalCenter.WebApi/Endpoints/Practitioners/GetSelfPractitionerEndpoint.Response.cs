namespace MedicalCenter.WebApi.Endpoints.Practitioners;

/// <summary>
/// Response DTO for getting current practitioner's custom attributes.
/// </summary>
public class GetSelfPractitionerResponse
{
    /// <summary>
    /// The practitioner's role.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    // Doctor attributes
    /// <summary>
    /// Doctor's license number (Doctor role only).
    /// </summary>
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// Doctor's specialty (Doctor role only).
    /// </summary>
    public string? Specialty { get; set; }

    // HealthcareStaff attributes
    /// <summary>
    /// Healthcare staff's organization name (HealthcareStaff role only).
    /// </summary>
    public string? OrganizationName { get; set; }

    /// <summary>
    /// Healthcare staff's or SystemAdmin's department (HealthcareStaff or SystemAdmin role only).
    /// </summary>
    public string? Department { get; set; }

    // Laboratory attributes
    /// <summary>
    /// Laboratory's lab name (LabUser role only).
    /// </summary>
    public string? LabName { get; set; }

    // ImagingCenter attributes
    /// <summary>
    /// Imaging center's center name (ImagingUser role only).
    /// </summary>
    public string? CenterName { get; set; }

    // SystemAdmin attributes
    /// <summary>
    /// System admin's corporate ID (SystemAdmin role only).
    /// </summary>
    public string? CorporateId { get; set; }
}


