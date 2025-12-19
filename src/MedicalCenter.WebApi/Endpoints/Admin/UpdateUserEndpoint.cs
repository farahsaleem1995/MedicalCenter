using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using MedicalCenter.Core.Aggregates.Doctors;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Core.Aggregates.Laboratories;
using MedicalCenter.Core.Aggregates.ImagingCenters;
using MedicalCenter.Core.Aggregates.SystemAdmins;
using MedicalCenter.Core.Authorization;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.Queries;
using MedicalCenter.Infrastructure.Data;

namespace MedicalCenter.WebApi.Endpoints.Admin;

/// <summary>
/// Admin endpoint to update a user.
/// </summary>
public class UpdateUserEndpoint(
    IUserQueryService userQueryService,
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IAuthorizationService authorizationService,
    MedicalCenterDbContext context)
    : Endpoint<UpdateUserRequest>
{
    public override void Configure()
    {
        Put("/users/{id}");
        Group<AdminGroup>();
        Policies(AuthorizationPolicies.RequireAdmin);
        Summary(s =>
        {
            s.Summary = "Update user";
            s.Description = "Allows system admin to update user details";
            s.Responses[204] = "User updated successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - Admin access required";
            s.Responses[404] = "User not found";
        });
    }

    public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
    {
        // Use admin method to ignore query filters (can update deactivated users)
        var user = await userQueryService.GetUserByIdAdminAsync(req.Id, ct);

        if (user == null)
        {
            ThrowError("User not found", 404);
            return;
        }

        // Business rule: SystemAdmin can only be updated by Super Admins
        if (user.Role == UserRole.SystemAdmin)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(
                User, 
                AuthorizationPolicies.CanManageAdmins);
            
            if (!authorizationResult.Succeeded)
            {
                ThrowError("Only Super Administrators can update SystemAdmin accounts.", 403);
                return;
            }
        }

        // Update common properties
        if (!string.IsNullOrWhiteSpace(req.FullName))
        {
            user.UpdateFullName(req.FullName);
        }

        // Update role-specific properties
        switch (user)
        {
            case Doctor doctor when !string.IsNullOrWhiteSpace(req.Specialty):
                doctor.UpdateSpecialty(req.Specialty);
                break;
            case HealthcareStaff healthcare when !string.IsNullOrWhiteSpace(req.OrganizationName) && !string.IsNullOrWhiteSpace(req.Department):
                healthcare.UpdateOrganization(req.OrganizationName, req.Department);
                break;
            case Laboratory lab when !string.IsNullOrWhiteSpace(req.LabName):
                lab.UpdateLabName(req.LabName);
                break;
            case ImagingCenter imaging when !string.IsNullOrWhiteSpace(req.CenterName):
                imaging.UpdateCenterName(req.CenterName);
                break;
            case SystemAdmin systemAdmin:
                if (!string.IsNullOrWhiteSpace(req.CorporateId))
                {
                    systemAdmin.UpdateCorporateId(req.CorporateId);
                }
                if (!string.IsNullOrWhiteSpace(req.Department))
                {
                    systemAdmin.UpdateDepartment(req.Department);
                }
                break;
        }

        context.Update(user);
        await context.SaveChangesAsync(ct);

        // Revoke all refresh tokens for this user
        await tokenProvider.RevokeUserRefreshTokensAsync(user.Id, ct);

        await Send.NoContentAsync(ct);
    }
}

