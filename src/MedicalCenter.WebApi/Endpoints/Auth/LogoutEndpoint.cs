using FastEndpoints;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Extensions;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Logout endpoint - invalidates all refresh tokens for the authenticated user.
/// </summary>
public class LogoutEndpoint(
    IIdentityService identityService)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/auth/logout");
        Group<AuthGroup>();
        Summary(s =>
        {
            s.Summary = "Logout user";
            s.Description = "Invalidates all refresh tokens for the authenticated user, requiring re-authentication";
            s.Responses[204] = "Logout successful";
            s.Responses[401] = "Unauthorized";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            ThrowError("Invalid user authentication", 401);
            return;
        }

        var result = await identityService.InvalidateUserRefreshTokensAsync(userId, ct);

        if (result.IsFailure)
        {
            int statusCode = result.Error!.Code.ToStatusCode();
            ThrowError(result.Error.Message, statusCode);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}

