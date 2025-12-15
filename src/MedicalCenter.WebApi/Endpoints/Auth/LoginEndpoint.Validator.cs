using FastEndpoints;
using FluentValidation;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Validator for login request.
/// Ensures email and password are provided and properly formatted.
/// </summary>
public class LoginRequestValidator : Validator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(256)
            .WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}

