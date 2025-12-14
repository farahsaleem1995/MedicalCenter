using MedicalCenter.Core.Entities;
using MedicalCenter.Core.Enums;

namespace MedicalCenter.WebApi.Endpoints.Admin;

public class GetUserResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

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

    public static GetUserResponse FromUser(User user)
    {
        var response = new GetUserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        // Set role-specific properties
        switch (user)
        {
            case Doctor doctor:
                response.LicenseNumber = doctor.LicenseNumber;
                response.Specialty = doctor.Specialty;
                break;
            case HealthcareEntity healthcare:
                response.OrganizationName = healthcare.OrganizationName;
                response.Department = healthcare.Department;
                break;
            case Laboratory lab:
                response.LabName = lab.LabName;
                response.LicenseNumber = lab.LicenseNumber;
                break;
            case ImagingCenter imaging:
                response.CenterName = imaging.CenterName;
                response.LicenseNumber = imaging.LicenseNumber;
                break;
        }

        return response;
    }
}

