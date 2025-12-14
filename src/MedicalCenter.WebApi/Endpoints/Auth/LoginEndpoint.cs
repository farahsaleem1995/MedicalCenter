using FastEndpoints;
using Microsoft.Extensions.Options;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Extensions;
using MedicalCenter.Infrastructure;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Login endpoint - authenticates user and returns JWT token.
/// </summary>
public class LoginEndpoint(
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IOptions<JwtSettings> jwtSettings)
    : Endpoint<LoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();
        Group<AuthGroup>();
        Summary(s =>
        {
            s.Summary = "Authenticate user and receive JWT token";
            s.Description = "Validates user credentials and returns access token and refresh token";
            s.Responses[200] = "Authentication successful";
            s.Responses[401] = "Invalid credentials";
        });
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var result = await identityService.ValidateCredentialsAsync(req.Email, req.Password, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error!.Code.ToStatusCode();
            ThrowError(result.Error.Message, statusCode);
            return;
        }

        var user = result.Value!;
        var token = tokenProvider.GenerateAccessToken(user);
        var refreshToken = tokenProvider.GenerateRefreshToken();

        // Save refresh token to database
        var expiryDate = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationInDays);
        var saveResult = await identityService.SaveRefreshTokenAsync(
            refreshToken,
            user.Id,
            expiryDate,
            ct);

        if (saveResult.IsFailure)
        {
            ThrowError("Failed to save refresh token", 500);
            return;
        }

        await Send.OkAsync(new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString()
        }, ct);
    }
}
