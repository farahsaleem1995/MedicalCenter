using FastEndpoints;
using FluentValidation;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.WebApi.Attributes;
using MedicalCenter.WebApi.Extensions;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Change password endpoint for authenticated users.
/// </summary>
[Command]
public class ChangePasswordEndpoint(
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IUserContext userContext)
    : Endpoint<ChangePasswordRequest>
{
    public override void Configure()
    {
        Put("/password");
        Group<AuthGroup>();
        Summary(s =>
        {
            s.Summary = "Change password";
            s.Description = "Changes the authenticated user's password by verifying the current password and setting a new one.";
            s.Responses[204] = "Password changed successfully";
            s.Responses[400] = "Invalid current password or validation error";
            s.Responses[401] = "Unauthorized";
        });
    }

    public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        Guid userId = userContext.UserId;

        var changeResult = await identityService.ChangePasswordAsync(userId, req.CurrentPassword, req.NewPassword, ct);
        if (changeResult.IsFailure)
        {
            int statusCode = changeResult.Error!.Code.ToStatusCode();
            ThrowError(changeResult.Error.Message, statusCode);
            return;
        }

        // Revoke all refresh tokens to force re-authentication
        await tokenProvider.RevokeUserRefreshTokensAsync(userId, ct);

        await Send.NoContentAsync(ct);
    }
}

/// <summary>
/// Request DTO for password change.
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Validator for ChangePasswordRequest.
/// </summary>
public class ChangePasswordRequestValidator : Validator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("New password must be at least 8 characters long.");
    }
}

