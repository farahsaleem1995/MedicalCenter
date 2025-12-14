using FastEndpoints;
using MedicalCenter.Core.Common;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Authorization;
using MedicalCenter.WebApi.Extensions;

namespace MedicalCenter.WebApi.Endpoints.Admin;

/// <summary>
/// Admin endpoint to change a user's password (without requiring current password).
/// </summary>
public class ChangePasswordEndpoint(
    IIdentityService identityService)
    : Endpoint<ChangePasswordRequest, ChangePasswordResponse>
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
            s.Responses[200] = "Password changed successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - Admin access required";
            s.Responses[404] = "User not found";
        });
    }

    public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        var result = await identityService.AdminChangePasswordAsync(req.Id, req.NewPassword, ct);

        if (result.IsFailure)
        {
            int statusCode = result.Error!.Code.ToStatusCode();
            ThrowError(result.Error.Message, statusCode);
            return;
        }

        Response = new ChangePasswordResponse
        {
            UserId = req.Id,
            Message = "Password changed successfully"
        };
    }
}

