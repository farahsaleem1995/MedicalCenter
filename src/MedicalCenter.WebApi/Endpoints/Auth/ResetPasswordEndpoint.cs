using FastEndpoints;
using FluentValidation;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Extensions;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Reset password endpoint.
/// </summary>
public class ResetPasswordEndpoint(
    IIdentityService identityService,
    ITokenProvider tokenProvider)
    : Endpoint<ResetPasswordRequest>
{
    public override void Configure()
    {
        Post("/password-reset");
        Group<AuthGroup>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Reset password";
            s.Description = "Resets a user's password using the 6-digit OTP code sent via email.";
            s.Responses[204] = "Password reset successfully";
            s.Responses[400] = "Invalid or expired code";
            s.Responses[404] = "User not found";
        });
    }

    public override async Task HandleAsync(ResetPasswordRequest req, CancellationToken ct)
    {
        // Find user by email
        Guid? userId = await identityService.GetUserByEmailAsync(req.Email, ct);
        if (!userId.HasValue)
        {
            ThrowError("User not found", 404);
            return;
        }

        // Reset password using OTP code (via Identity service)
        var resetResult = await identityService.ResetPasswordAsync(userId.Value, req.Code, req.NewPassword, ct);
        if (resetResult.IsFailure)
        {
            int statusCode = resetResult.Error!.Code.ToStatusCode();
            ThrowError(resetResult.Error.Message, statusCode);
            return;
        }

        // Revoke all refresh tokens to force re-authentication
        await tokenProvider.RevokeUserRefreshTokensAsync(userId.Value, ct);

        await Send.NoContentAsync(ct);
    }
}

/// <summary>
/// Request DTO for password reset.
/// </summary>
public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Validator for ResetPasswordRequest.
/// </summary>
public class ResetPasswordRequestValidator : Validator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6)
            .Matches(@"^\d{6}$")
            .WithMessage("Code must be a 6-digit number.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.");
    }
}

