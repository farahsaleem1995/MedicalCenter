using FastEndpoints;
using FluentValidation;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Request email confirmation endpoint for patients.
/// </summary>
public class RequestEmailConfirmationEndpoint(
    IRepository<Patient> patientRepository,
    IIdentityService identityService,
    ISmtpClient smtpClient)
    : Endpoint<RequestEmailConfirmationRequest>
{
    public override void Configure()
    {
        Get("/confirm");
        Group<AuthGroup>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Request email confirmation";
            s.Description = "Sends a confirmation email to the patient. The patient must use the 6-digit code from the email to confirm their account.";
            s.Responses[204] = "Confirmation email sent successfully";
            s.Responses[400] = "Validation error";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(RequestEmailConfirmationRequest req, CancellationToken ct)
    {
        // Find patient by email
        var specification = new PatientByEmailSpecification(req.Email);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        // Check if already confirmed (via Identity service)
        var isUnconfirmed = await identityService.IsUserUnconfirmedAsync(patient.Id, ct);
        if (!isUnconfirmed)
        {
            ThrowError("Email is already confirmed", 400);
            return;
        }

        // Generate 6-digit OTP code (via Identity service)
        var codeResult = await identityService.GenerateEmailConfirmationCodeAsync(patient.Id, ct);
        if (codeResult.IsFailure)
        {
            ThrowError(codeResult.Error!.Message, 400);
            return;
        }

        var code = codeResult.Value!;

        // Generate email body with OTP code
        var emailBody = GenerateConfirmationEmailBody(patient.FullName, code);

        // Send confirmation email
        var sendResult = await smtpClient.SendEmailAsync(
            patient.Email,
            "Confirm Your Email - Medical Center",
            emailBody,
            ct);

        if (sendResult.IsFailure)
        {
            // Log error but don't fail the request - code is already generated
            // Client can request again if email fails
            ThrowError("Failed to send confirmation email. Please try again.", 500);
            return;
        }

        await Send.NoContentAsync(ct);
    }

    private static string GenerateConfirmationEmailBody(string fullName, string code)
    {
        return $@"
            <html>
            <body>
                <h2>Email Confirmation</h2>
                <p>Hello {fullName},</p>
                <p>Your email confirmation code is:</p>
                <h3 style=""font-size: 24px; letter-spacing: 5px; color: #2563eb;"">{code}</h3>
                <p>This code will expire in up to 9 minutes.</p>
                <p>If you did not request this confirmation, please ignore this email.</p>
            </body>
            </html>";
    }
}

/// <summary>
/// Request DTO for email confirmation request.
/// </summary>
public class RequestEmailConfirmationRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Validator for RequestEmailConfirmationRequest.
/// </summary>
public class RequestEmailConfirmationRequestValidator : Validator<RequestEmailConfirmationRequest>
{
    public RequestEmailConfirmationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}

