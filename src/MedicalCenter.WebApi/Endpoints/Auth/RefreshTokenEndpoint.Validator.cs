using FluentValidation;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Validator for refresh token request.
/// Ensures refresh token is provided.
/// </summary>
public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.");
    }
}

