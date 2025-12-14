using FastEndpoints;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Authorization;
using MedicalCenter.Infrastructure.Data;

namespace MedicalCenter.WebApi.Endpoints.Admin;

/// <summary>
/// Admin endpoint to delete/deactivate a user.
/// </summary>
public class DeleteUserEndpoint(
    IUserQueryService userQueryService,
    MedicalCenterDbContext context)
    : Endpoint<DeleteUserRequest, DeleteUserResponse>
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
            s.Responses[200] = "User deactivated successfully";
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

        // Soft delete - deactivate the user
        user.Deactivate();
        context.Update(user);
        await context.SaveChangesAsync(ct);

        Response = new DeleteUserResponse
        {
            UserId = user.Id,
            Message = "User deactivated successfully"
        };
    }
}

