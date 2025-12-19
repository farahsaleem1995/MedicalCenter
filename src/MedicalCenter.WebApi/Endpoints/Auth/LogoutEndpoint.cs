using FastEndpoints;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Extensions;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Logout endpoint - invalidates all refresh tokens for the authenticated user.
/// </summary>
public class LogoutEndpoint(
    IIdentityService identityService,
    IUserContext userContext)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/logout");
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
        var userId = userContext.UserId;

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

