using MedicalCenter.Core.Aggregates;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Common;

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

    public PatientDetails? PatientDetails { get; set; }
    public DoctorDetails? DoctorDetails { get; set; }
    public HealthcareEntityDetails? HealthcareEntityDetails { get; set; }
    public LaboratoryDetails? LaboratoryDetails { get; set; }
    public ImagingCenterDetails? ImagingCenterDetails { get; set; }

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
            case Patient patient:
                response.PatientDetails = new PatientDetails
                {
                    NationalId = patient.NationalId,
                    DateOfBirth = patient.DateOfBirth,
                    BloodType = patient.BloodType
                };
                break;
            case Doctor doctor:
                response.DoctorDetails = new DoctorDetails
                {
                    LicenseNumber = doctor.LicenseNumber,
                    Specialty = doctor.Specialty
                };
                break;
            case HealthcareEntity healthcare:
                response.HealthcareEntityDetails = new HealthcareEntityDetails
                {
                    OrganizationName = healthcare.OrganizationName,
                    Department = healthcare.Department
                };
                break;
            case Laboratory lab:
                response.LaboratoryDetails = new LaboratoryDetails
                {
                    LabName = lab.LabName
                };
                break;
            case ImagingCenter imaging:
                response.ImagingCenterDetails = new ImagingCenterDetails
                {
                    CenterName = imaging.CenterName
                };
                break;
        }

        return response;
    }
}

public class PatientDetails
{
    public string NationalId { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public BloodType? BloodType { get; set; }
}

public class DoctorDetails
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
}

public class HealthcareEntityDetails
{
    public string OrganizationName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

public class LaboratoryDetails
{
    public string LabName { get; set; } = string.Empty;
}

public class ImagingCenterDetails
{
    public string CenterName { get; set; } = string.Empty;
}

