using FastEndpoints;
using Microsoft.Extensions.Options;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Extensions;
using MedicalCenter.Infrastructure;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Refresh token endpoint - exchanges a refresh token for a new access token.
/// </summary>
public class RefreshTokenEndpoint(
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IOptions<JwtSettings> jwtSettings)
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
        var validationResult = await identityService.ValidateRefreshTokenAsync(req.RefreshToken, ct);
        if (validationResult.IsFailure)
        {
            var statusCode = validationResult.Error!.Code.ToStatusCode();
            ThrowError(validationResult.Error.Message, statusCode);
            return;
        }

        var userId = validationResult.Value!;

        // Get user
        var user = await identityService.GetUserByIdAsync(userId, ct);
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

        // Generate new tokens
        var newToken = tokenProvider.GenerateAccessToken(user);
        var newRefreshToken = tokenProvider.GenerateRefreshToken();

        // Revoke old refresh token and save new one
        await identityService.RevokeRefreshTokenAsync(req.RefreshToken, ct);
        var expiryDate = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationInDays);
        var saveResult = await identityService.SaveRefreshTokenAsync(
            newRefreshToken,
            userId,
            expiryDate,
            ct);

        if (saveResult.IsFailure)
        {
            ThrowError("Failed to save refresh token", 500);
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

