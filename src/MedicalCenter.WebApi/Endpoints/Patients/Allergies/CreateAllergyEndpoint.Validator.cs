using FastEndpoints;
using FluentValidation;

namespace MedicalCenter.WebApi.Endpoints.Patients.Allergies;

/// <summary>
/// Validator for create allergy request.
/// </summary>
public class CreateAllergyEndpointValidator : Validator<CreateAllergyRequest>
{
    public CreateAllergyEndpointValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("Patient ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Allergy name is required.")
            .MaximumLength(200)
            .WithMessage("Allergy name cannot exceed 200 characters.");

        RuleFor(x => x.Severity)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Severity))
            .WithMessage("Severity cannot exceed 50 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 1000 characters.");
    }
}

