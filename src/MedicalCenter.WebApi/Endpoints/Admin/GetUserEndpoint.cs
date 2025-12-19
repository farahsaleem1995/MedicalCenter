using FastEndpoints;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Queries;
using MedicalCenter.Core.Authorization;
using MedicalCenter.WebApi.Attributes;
using MedicalCenter.WebApi.Extensions;

namespace MedicalCenter.WebApi.Endpoints.Admin;

/// <summary>
/// Admin endpoint to get a user by ID.
/// </summary>
[Query]
public class GetUserEndpoint(
    IUserQueryService userQueryService)
    : Endpoint<GetUserRequest, GetUserResponse>
{
    public override void Configure()
    {
        Get("/users/{id}");
        Group<AdminGroup>();
        Policies(AuthorizationPolicies.RequireAdmin);
        Summary(s =>
        {
            s.Summary = "Get user by ID";
            s.Description = "Allows system admin to retrieve user details by ID. All SystemAdmin users can view SystemAdmin accounts. Only Super Admins can modify them.";
            s.Responses[200] = "User found";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - Admin access required";
            s.Responses[404] = "User not found";
        });
    }

    public override async Task HandleAsync(GetUserRequest req, CancellationToken ct)
    {
        // Use admin method to ignore query filters (include deactivated users)
        var user = await userQueryService.GetUserByIdAdminAsync(req.Id, ct);

        if (user == null)
        {
            ThrowError("User not found", 404);
            return;
        }

        // All SystemAdmin users can retrieve/view SystemAdmin accounts
        // Only Super Admins (with CanManageAdmins policy) can modify them
        await Send.OkAsync(GetUserResponse.FromUser(user), ct);
    }
}

