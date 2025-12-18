using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Doctors;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Core.Aggregates.Laboratories;
using MedicalCenter.Core.Aggregates.ImagingCenters;
using MedicalCenter.Core.Aggregates.SystemAdmins;
using MedicalCenter.Core.Aggregates.Patients.ValueObjects;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;

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
    public HealthcareStaffDetails? HealthcareStaffDetails { get; set; }
    public LaboratoryDetails? LaboratoryDetails { get; set; }
    public ImagingCenterDetails? ImagingCenterDetails { get; set; }
    public SystemAdminDetails? SystemAdminDetails { get; set; }

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
            case HealthcareStaff healthcare:
                response.HealthcareStaffDetails = new HealthcareStaffDetails
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
            case SystemAdmin systemAdmin:
                response.SystemAdminDetails = new SystemAdminDetails
                {
                    CorporateId = systemAdmin.CorporateId,
                    Department = systemAdmin.Department
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

public class HealthcareStaffDetails
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

public class SystemAdminDetails
{
    public string CorporateId { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

