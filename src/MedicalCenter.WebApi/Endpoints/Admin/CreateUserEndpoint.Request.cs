using MedicalCenter.Core.Enums;

namespace MedicalCenter.WebApi.Endpoints.Admin;

public class CreateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    // Doctor-specific
    public string? LicenseNumber { get; set; }
    public string? Specialty { get; set; }

    // HealthcareStaff-specific
    public string? OrganizationName { get; set; }
    public string? Department { get; set; }

    // LabUser-specific
    public string? LabName { get; set; }

    // ImagingUser-specific
    public string? CenterName { get; set; }
}

