using FastEndpoints;
using MedicalCenter.Core.Common;
using MedicalCenter.Core.Services;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Get current authenticated user's generic information endpoint.
/// </summary>
public class GetSelfEndpoint(
    IIdentityService identityService)
    : EndpointWithoutRequest<GetSelfResponse>
{
    public override void Configure()
    {
        Get("/self");
        Group<AuthGroup>();
        Summary(s =>
        {
            s.Summary = "Get current user information";
            s.Description = "Returns the authenticated user's generic information (ID, email, full name, role, active status)";
            s.Responses[200] = "User information retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[404] = "User not found";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string? userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            ThrowError("Invalid user authentication", 401);
            return;
        }

        User? user = await identityService.GetUserByIdAsync(userId, ct);

        if (user == null)
        {
            ThrowError("User not found", 404);
            return;
        }

        await Send.OkAsync(new GetSelfResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString()
        }, ct);
    }
}

