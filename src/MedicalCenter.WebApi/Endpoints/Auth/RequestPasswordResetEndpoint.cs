using FastEndpoints;
using FluentValidation;
using MedicalCenter.Core.Queries;
using MedicalCenter.Core.Services;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Request password reset endpoint.
/// </summary>
public class RequestPasswordResetEndpoint(
    IIdentityService identityService,
    IUserQueryService userQueryService,
    ISmtpClient smtpClient)
    : Endpoint<RequestPasswordResetRequest>
{
    public override void Configure()
    {
        Get("/password-reset");
        Group<AuthGroup>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Request password reset";
            s.Description = "Sends a password reset code to the user's email. The user must use the 6-digit code to reset their password.";
            s.Responses[204] = "Password reset code sent successfully";
            s.Responses[400] = "Validation error";
            s.Responses[404] = "User not found";
        });
    }

    public override async Task HandleAsync(RequestPasswordResetRequest req, CancellationToken ct)
    {
        // Find user by email
        Guid? userId = await identityService.GetUserByEmailAsync(req.Email, ct);
        if (!userId.HasValue)
        {
            ThrowError("User not found", 404);
            return;
        }

        // Generate 6-digit OTP code (via Identity service)
        var codeResult = await identityService.GeneratePasswordResetCodeAsync(userId.Value, ct);
        if (codeResult.IsFailure)
        {
            ThrowError(codeResult.Error!.Message, 400);
            return;
        }

        // Generate email body with OTP code
        var emailBody = await GeneratePasswordResetEmailBody(userId.Value, codeResult.Value!, ct);

        // Send password reset email
        var sendResult = await smtpClient.SendEmailAsync(
            req.Email,
            "Password Reset - Medical Center",
            emailBody,
            ct);

        if (sendResult.IsFailure)
        {
            // Log error but don't fail the request - code is already generated
            // Client can request again if email fails
            ThrowError("Failed to send password reset email. Please try again.", 500);
            return;
        }

        await Send.NoContentAsync(ct);
    }

    private async Task<string> GeneratePasswordResetEmailBody(Guid userId, string code, CancellationToken ct)
    {
        var user = await userQueryService.GetUserByIdAsync(userId, ct);
        if (user == null)
        {
            throw new InvalidCastException("Failed to generate password reset email body. User not found.");
        }
        
        return $@"
            <html>
            <body>
                <h2>Password Reset</h2>
                <p>Hello {user.FullName},</p>
                <p>Your password reset code is:</p>
                <h3 style=""font-size: 24px; letter-spacing: 5px; color: #2563eb;"">{code}</h3>
                <p>This code will expire in up to 9 minutes.</p>
                <p>If you did not request a password reset, please ignore this email.</p>
            </body>
            </html>";
    }
}

/// <summary>
/// Request DTO for password reset request.
/// </summary>
public class RequestPasswordResetRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Validator for RequestPasswordResetRequest.
/// </summary>
public class RequestPasswordResetRequestValidator : Validator<RequestPasswordResetRequest>
{
    public RequestPasswordResetRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}

