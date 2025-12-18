using FastEndpoints;
using FluentValidation;
using MedicalCenter.Core.Services;

namespace MedicalCenter.WebApi.Endpoints.Patients.Surgeries;

/// <summary>
/// Validator for create surgery request.
/// </summary>
public class CreateSurgeryEndpointValidator : Validator<CreateSurgeryRequest>
{
    public CreateSurgeryEndpointValidator(IDateTimeProvider dateTimeProvider)
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
            .LessThanOrEqualTo(dateTimeProvider.Now)
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

