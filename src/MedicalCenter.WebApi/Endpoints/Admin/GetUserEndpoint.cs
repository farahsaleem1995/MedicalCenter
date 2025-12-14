using FastEndpoints;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Authorization;
using MedicalCenter.WebApi.Extensions;

namespace MedicalCenter.WebApi.Endpoints.Admin;

/// <summary>
/// Admin endpoint to get a user by ID.
/// </summary>
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
            s.Description = "Allows system admin to retrieve user details by ID";
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

        Response = GetUserResponse.FromUser(user);
    }
}

