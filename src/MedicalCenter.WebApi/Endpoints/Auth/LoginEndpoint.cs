using FastEndpoints;
using MedicalCenter.Core.Queries;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Extensions;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Login endpoint - authenticates user and returns JWT token.
/// </summary>
public class LoginEndpoint(
    IIdentityService identityService,
    IUserQueryService userQueryService,
    ITokenProvider tokenProvider)
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

        var userId = result.Value!;

        // Check email confirmation - users with unconfirmed email cannot login
        var isUnconfirmed = await identityService.IsUserUnconfirmedAsync(userId, ct);
        if (isUnconfirmed)
        {
            ThrowError("Email address must be confirmed before logging in. Please check your email for the confirmation code.", 403);
            return;
        }

        var token = await tokenProvider.GenerateAccessTokenAsync(userId, ct);
        var refreshToken = await tokenProvider.GenerateRefreshTokenAsync(userId, ct);

        var user = await userQueryService.GetUserByIdAsync(userId, ct);
        if (user == null)
        {
            ThrowError("Invalid login.", 401);
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
