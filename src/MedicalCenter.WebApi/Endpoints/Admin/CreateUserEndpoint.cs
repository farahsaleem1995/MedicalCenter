using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using MedicalCenter.Core.Authorization;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure;
using MedicalCenter.WebApi.Attributes;
using MedicalCenter.WebApi.Extensions;

namespace MedicalCenter.WebApi.Endpoints.Admin;

/// <summary>
/// Admin endpoint to create users (non-patients).
/// </summary>
[Command]
public class CreateUserEndpoint(
    IIdentityService identityService,
    IAuthorizationService authorizationService)
    : Endpoint<CreateUserRequest, CreateUserResponse>
{
    public override void Configure()
    {
        Post("/users");
        Group<AdminGroup>();
        Policies(AuthorizationPolicies.RequireAdmin);
        Summary(s =>
        {
            s.Summary = "Create a new user (non-patient)";
            s.Description = "Allows system admin to create users of practitioner types (Doctor, HealthcareStaff, LabUser, ImagingUser). SystemAdmin accounts can only be created by Super Administrators.";
            s.Responses[200] = "User created successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - Admin access required or insufficient privileges for SystemAdmin creation";
            s.Responses[409] = "User already exists";
        });
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        // Business rule: SystemAdmin can only be created by Super Admins
        if (req.Role == UserRole.SystemAdmin)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(
                User, 
                AuthorizationPolicies.CanManageAdmins);
            
            if (!authorizationResult.Succeeded)
            {
                ThrowError("Only Super Administrators can create SystemAdmin accounts.", 403);
                return;
            }
        }

        Result<Guid> result = req.Role switch
        {
            UserRole.Doctor => await identityService.CreateDoctorAsync(
                req.FullName,
                req.Email,
                req.Password,
                req.LicenseNumber!,
                req.Specialty!,
                ct),
            UserRole.HealthcareStaff => await identityService.CreateHealthcareStaffAsync(
                req.FullName,
                req.Email,
                req.Password,
                req.OrganizationName!,
                req.Department!,
                ct),
            UserRole.LabUser => await identityService.CreateLaboratoryAsync(
                req.FullName,
                req.Email,
                req.Password,
                req.LabName!,
                req.LicenseNumber!,
                ct),
            UserRole.ImagingUser => await identityService.CreateImagingCenterAsync(
                req.FullName,
                req.Email,
                req.Password,
                req.CenterName!,
                req.LicenseNumber!,
                ct),
            UserRole.SystemAdmin => await identityService.CreateSystemAdminAsync(
                req.FullName,
                req.Email,
                req.Password,
                req.CorporateId!,
                req.Department!,
                ct),
            _ => Result<Guid>.Failure(Error.Validation("Invalid role. Only practitioner roles and SystemAdmin are allowed."))
        };

        if (result.IsFailure)
        {
            int statusCode = result.Error!.Code.ToStatusCode();
            ThrowError(result.Error.Message, statusCode);
            return;
        }

        await Send.OkAsync(new CreateUserResponse
        {
            UserId = result.Value,
            Email = req.Email,
            FullName = req.FullName,
            Role = req.Role.ToString()
        }, ct);
    }
}

