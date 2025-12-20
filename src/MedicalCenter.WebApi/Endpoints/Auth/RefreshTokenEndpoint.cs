using FastEndpoints;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Extensions;
using MedicalCenter.Core.Queries;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Refresh token endpoint - exchanges a refresh token for a new access token.
/// </summary>
public class RefreshTokenEndpoint(
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IUserQueryService userQueryService)
    : Endpoint<RefreshTokenRequest, RefreshTokenResponse>
{
    public override void Configure()
    {
        Post("/refresh");
        AllowAnonymous();
        Group<AuthGroup>();
        Summary(s =>
        {
            s.Summary = "Refresh access token";
            s.Description = "Exchanges a valid refresh token for a new access token and refresh token";
            s.Responses[200] = "Token refresh successful";
            s.Responses[401] = "Invalid or expired refresh token";
        });
    }

    public override async Task HandleAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        // Validate refresh token
        var validationResult = await tokenProvider.ValidateRefreshTokenAsync(req.RefreshToken, ct);
        if (validationResult.IsFailure)
        {
            var statusCode = validationResult.Error!.Code.ToStatusCode();
            ThrowError(validationResult.Error.Message, statusCode);
            return;
        }

        var userId = validationResult.Value!;

        // Get user
        var user = await userQueryService.GetUserByIdAsync(userId, ct);
        if (user == null)
        {
            ThrowError("User not found", 404);
            return;
        }

        if (!user.IsActive)
        {
            ThrowError("User account is inactive", 401);
            return;
        }

        // Check email confirmation - users with unconfirmed email cannot refresh tokens
        var isUnconfirmed = await identityService.IsUserUnconfirmedAsync(userId, ct);
        if (isUnconfirmed)
        {
            // Revoke the refresh token since user is unconfirmed
            await tokenProvider.RevokeRefreshTokenAsync(req.RefreshToken, ct);
            ThrowError("Email address must be confirmed before accessing the system. Please check your email for the confirmation code.", 403);
            return;
        }

        // Generate new tokens
        var newToken = await tokenProvider.GenerateAccessTokenAsync(user.Id, ct);
        var newRefreshToken = await tokenProvider.GenerateRefreshTokenAsync(user.Id, ct);

        var revokeResult = await tokenProvider.RevokeRefreshTokenAsync(req.RefreshToken, ct);
        if (revokeResult.IsFailure)
        {
            ThrowError("Failed to revoke refresh token", 500);
            return;
        }

        await Send.OkAsync(new RefreshTokenResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString()
        }, ct);
    }
}

