using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using MedicalCenter.Core.Authorization;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Queries;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.WebApi.Attributes;

namespace MedicalCenter.WebApi.Endpoints.Admin;

/// <summary>
/// Admin endpoint to delete/deactivate a user.
/// </summary>
[ActionLog("System administrator deactivated a user account")]
public class DeleteUserEndpoint(
    IUserQueryService userQueryService,
    IAuthorizationService authorizationService,
    ITokenProvider tokenProvider,
    MedicalCenterDbContext context)
    : Endpoint<DeleteUserRequest>
{
    public override void Configure()
    {
        Delete("/users/{id}");
        Group<AdminGroup>();
        Policies(AuthorizationPolicies.RequireAdmin);
        Summary(s =>
        {
            s.Summary = "Delete/deactivate user";
            s.Description = "Allows system admin to deactivate a user (soft delete)";
            s.Responses[204] = "User deactivated successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - Admin access required";
            s.Responses[404] = "User not found";
        });
    }

    public override async Task HandleAsync(DeleteUserRequest req, CancellationToken ct)
    {
        // Use admin method to ignore query filters (can deactivate already deactivated users)
        var user = await userQueryService.GetUserByIdAdminAsync(req.Id, ct);
        if (user == null)
        {
            ThrowError("User not found", 404);
            return;
        }

        // Business rule: SystemAdmin can only be deleted by Super Admins
        if (user.Role == UserRole.SystemAdmin)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(
                User, 
                AuthorizationPolicies.CanManageAdmins);
            
            if (!authorizationResult.Succeeded)
            {
                ThrowError("Only Super Administrators can delete SystemAdmin accounts.", 403);
                return;
            }
        }

        // Soft delete - deactivate the user
        user.Deactivate();
        context.Update(user);
        await context.SaveChangesAsync(ct);

        // Revoke all refresh tokens for this user
        await tokenProvider.RevokeUserRefreshTokensAsync(user.Id, ct);

        await Send.NoContentAsync(ct);
    }
}

