using FastEndpoints;
using FluentValidation;
using MedicalCenter.Core.Services;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Validator for patient registration request.
/// Enforces Identity password rules and validates all required fields.
/// </summary>
public class RegisterPatientRequestValidator : Validator<RegisterPatientRequest>
{
    public RegisterPatientRequestValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required.")
            .MaximumLength(200)
            .WithMessage("Full name must not exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(256)
            .WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.")
            .Must(ContainDigit)
            .WithMessage("Password must contain at least one digit.")
            .Must(ContainLowercase)
            .WithMessage("Password must contain at least one lowercase letter.")
            .Must(ContainUppercase)
            .WithMessage("Password must contain at least one uppercase letter.")
            .Must(ContainNonAlphanumeric)
            .WithMessage("Password must contain at least one non-alphanumeric character.");

        RuleFor(x => x.NationalId)
            .NotEmpty()
            .WithMessage("National ID is required.")
            .MaximumLength(50)
            .WithMessage("National ID must not exceed 50 characters.");

        var now = dateTimeProvider.Now;
        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .WithMessage("Date of birth is required.")
            .LessThan(now)
            .WithMessage("Date of birth must be in the past.")
            .Must(dob => dob.Date >= now.Date.AddYears(-150))
            .WithMessage("Date of birth must be within a reasonable range.");
    }

    private static bool ContainDigit(string password)
    {
        return password.Any(char.IsDigit);
    }

    private static bool ContainLowercase(string password)
    {
        return password.Any(char.IsLower);
    }

    private static bool ContainUppercase(string password)
    {
        return password.Any(char.IsUpper);
    }

    private static bool ContainNonAlphanumeric(string password)
    {
        return password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}

