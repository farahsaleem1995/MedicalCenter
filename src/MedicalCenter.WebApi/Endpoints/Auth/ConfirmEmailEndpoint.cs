using FastEndpoints;
using FluentValidation;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Extensions;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Confirm email endpoint for patients.
/// </summary>
public class ConfirmEmailEndpoint(
    IRepository<Patient> patientRepository,
    IIdentityService identityService)
    : Endpoint<ConfirmEmailRequest>
{
    public override void Configure()
    {
        Post("/confirm");
        Group<AuthGroup>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Confirm email address";
            s.Description = "Confirms a patient's email address using the 6-digit OTP code sent via email.";
            s.Responses[204] = "Email confirmed successfully";
            s.Responses[400] = "Invalid or expired code";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(ConfirmEmailRequest req, CancellationToken ct)
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

        // Confirm email using OTP code (via Identity service)
        var confirmResult = await identityService.ConfirmEmailAsync(patient.Id, req.Code, ct);
        if (confirmResult.IsFailure)
        {
            int statusCode = confirmResult.Error!.Code.ToStatusCode();
            ThrowError(confirmResult.Error.Message, statusCode);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}

/// <summary>
/// Request DTO for email confirmation.
/// </summary>
public class ConfirmEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// Validator for ConfirmEmailRequest.
/// </summary>
public class ConfirmEmailRequestValidator : Validator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
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
    }
}

