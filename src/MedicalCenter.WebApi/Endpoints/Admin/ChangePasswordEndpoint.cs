using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.Queries;
using MedicalCenter.Core.Authorization;
using MedicalCenter.WebApi.Extensions;
using MedicalCenter.WebApi.Attributes;

namespace MedicalCenter.WebApi.Endpoints.Admin;

/// <summary>
/// Admin endpoint to change a user's password (without requiring current password).
/// </summary>
[ActionLog("System administrator changed a user's password")]
public class ChangePasswordEndpoint(
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IUserQueryService userQueryService,
    IAuthorizationService authorizationService)
    : Endpoint<ChangePasswordRequest>
{
    public override void Configure()
    {
        Put("/users/{id}/password");
        Group<AdminGroup>();
        Policies(AuthorizationPolicies.RequireAdmin);
        Summary(s =>
        {
            s.Summary = "Change user password (admin)";
            s.Description = "Allows system admin to change a user's password without requiring the current password";
            s.Responses[204] = "Password changed successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - Admin access required";
            s.Responses[404] = "User not found";
        });
    }

    public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        // Check if the user being modified is a SystemAdmin
        var user = await userQueryService.GetUserByIdAsync(req.Id, ct);
        if (user == null)
        {
            ThrowError("User not found", 404);
            return;
        }

        // Business rule: SystemAdmin password can only be changed by Super Admins
        if (user.Role == UserRole.SystemAdmin)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(
                User, 
                AuthorizationPolicies.CanManageAdmins);
            
            if (!authorizationResult.Succeeded)
            {
                ThrowError("Only Super Administrators can change SystemAdmin passwords.", 403);
                return;
            }
        }

        var result = await identityService.UpdatePasswordAsync(req.Id, req.NewPassword, ct);

        if (result.IsFailure)
        {
            int statusCode = result.Error!.Code.ToStatusCode();
            ThrowError(result.Error.Message, statusCode);
            return;
        }

        // Revoke all refresh tokens to force re-authentication
        await tokenProvider.RevokeUserRefreshTokensAsync(req.Id, ct);

        await Send.NoContentAsync(ct);
    }
}

