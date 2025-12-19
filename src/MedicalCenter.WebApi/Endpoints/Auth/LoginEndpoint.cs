using FastEndpoints;
using Microsoft.Extensions.Options;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Attributes;
using MedicalCenter.WebApi.Extensions;
using MedicalCenter.Infrastructure;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Login endpoint - authenticates user and returns JWT token.
/// </summary>
[Query]
public class LoginEndpoint(
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IOptions<JwtSettings> jwtSettings,
    IDateTimeProvider dateTimeProvider)
    : Endpoint<LoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/login");
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

        // Check email confirmation - users with unconfirmed email cannot login
        var isUnconfirmed = await identityService.IsUserUnconfirmedAsync(user.Id, ct);
        if (isUnconfirmed)
        {
            ThrowError("Email address must be confirmed before logging in. Please check your email for the confirmation code.", 403);
            return;
        }
        var token = tokenProvider.GenerateAccessToken(user);
        var refreshToken = tokenProvider.GenerateRefreshToken();

        // Save refresh token to database
        var expiryDate = dateTimeProvider.Now.AddDays(jwtSettings.Value.RefreshTokenExpirationInDays);
        var saveResult = await tokenProvider.SaveRefreshTokenAsync(
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
