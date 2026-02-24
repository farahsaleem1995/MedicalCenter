namespace MedicalCenter.WebApi.Endpoints.Admin;

public class UpdateUserRequest
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? NationalId { get; set; }

    // Doctor-specific
    public string? Specialty { get; set; }

    // HealthcareStaff-specific
    public string? OrganizationName { get; set; }
    public string? Department { get; set; }

    // LabUser-specific
    public string? LabName { get; set; }

    // ImagingUser-specific
    public string? CenterName { get; set; }

    // SystemAdmin-specific
    public string? CorporateId { get; set; }
}

