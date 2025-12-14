using FluentValidation;

namespace MedicalCenter.WebApi.Endpoints.Patients.Surgeries;

/// <summary>
/// Validator for create surgery request.
/// </summary>
public class CreateSurgeryEndpointValidator : AbstractValidator<CreateSurgeryRequest>
{
    public CreateSurgeryEndpointValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("Patient ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Surgery name is required.")
            .MaximumLength(200)
            .WithMessage("Surgery name cannot exceed 200 characters.");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Surgery date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Surgery date cannot be in the future.");

        RuleFor(x => x.Surgeon)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Surgeon))
            .WithMessage("Surgeon name cannot exceed 200 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 1000 characters.");
    }
}

