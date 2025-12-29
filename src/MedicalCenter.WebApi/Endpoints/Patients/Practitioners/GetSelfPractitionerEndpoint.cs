using FastEndpoints;
using MedicalCenter.Core.Aggregates.Doctors;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Core.Aggregates.Laboratories;
using MedicalCenter.Core.Aggregates.ImagingCenters;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients.Practitioners;

/// <summary>
/// Get current practitioner's own custom attributes endpoint.
/// </summary>
public class GetSelfPractitionerEndpoint(
    IRepository<Doctor> doctorRepository,
    IRepository<HealthcareStaff> healthcareStaffRepository,
    IRepository<Laboratory> laboratoryRepository,
    IRepository<ImagingCenter> imagingCenterRepository,
    IUserContext userContext)
    : EndpointWithoutRequest<GetSelfPractitionerResponse>
{
    public override void Configure()
    {
        Get("/self");
        Group<PractitionersGroup>();
        Policies(AuthorizationPolicies.RequirePractitioner);
        Summary(s =>
        {
            s.Summary = "Get current practitioner's custom attributes";
            s.Description = "Returns the authenticated practitioner's custom attributes based on their role (Doctor, HealthcareStaff, Laboratory, or ImagingCenter)";
            s.Responses[200] = "Practitioner attributes retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - user is not a practitioner";
            s.Responses[404] = "Practitioner not found";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = userContext.UserId;
        var role = userContext.Role;

        GetSelfPractitionerResponse response = role switch
        {
            UserRole.Doctor => await GetDoctorAttributes(userId, ct),
            UserRole.HealthcareStaff => await GetHealthcareStaffAttributes(userId, ct),
            UserRole.LabUser => await GetLaboratoryAttributes(userId, ct),
            UserRole.ImagingUser => await GetImagingCenterAttributes(userId, ct),
            _ => throw new InvalidOperationException($"User role {role} is not a practitioner")
        };

        await Send.OkAsync(response, ct);
    }

    private async Task<GetSelfPractitionerResponse> GetDoctorAttributes(Guid userId, CancellationToken ct)
    {
        var doctor = await doctorRepository.GetByIdAsync(userId, ct);
        
        if (doctor == null)
        {
            ThrowError("Doctor not found", 404);
            return null!; // Will never be reached, but satisfies compiler
        }

        return new GetSelfPractitionerResponse
        {
            Role = "Doctor",
            LicenseNumber = doctor.LicenseNumber,
            Specialty = doctor.Specialty
        };
    }

    private async Task<GetSelfPractitionerResponse> GetHealthcareStaffAttributes(Guid userId, CancellationToken ct)
    {
        var healthcareStaff = await healthcareStaffRepository.GetByIdAsync(userId, ct);
        
        if (healthcareStaff == null)
        {
            ThrowError("Healthcare staff not found", 404);
            return null!; // Will never be reached, but satisfies compiler
        }

        return new GetSelfPractitionerResponse
        {
            Role = "HealthcareStaff",
            OrganizationName = healthcareStaff.OrganizationName,
            Department = healthcareStaff.Department
        };
    }

    private async Task<GetSelfPractitionerResponse> GetLaboratoryAttributes(Guid userId, CancellationToken ct)
    {
        var laboratory = await laboratoryRepository.GetByIdAsync(userId, ct);
        
        if (laboratory == null)
        {
            ThrowError("Laboratory not found", 404);
            return null!; // Will never be reached, but satisfies compiler
        }

        return new GetSelfPractitionerResponse
        {
            Role = "LabUser",
            LabName = laboratory.LabName,
            LicenseNumber = laboratory.LicenseNumber
        };
    }

    private async Task<GetSelfPractitionerResponse> GetImagingCenterAttributes(Guid userId, CancellationToken ct)
    {
        var imagingCenter = await imagingCenterRepository.GetByIdAsync(userId, ct);
        
        if (imagingCenter == null)
        {
            ThrowError("Imaging center not found", 404);
            return null!; // Will never be reached, but satisfies compiler
        }

        return new GetSelfPractitionerResponse
        {
            Role = "ImagingUser",
            CenterName = imagingCenter.CenterName,
            LicenseNumber = imagingCenter.LicenseNumber
        };
    }

}

