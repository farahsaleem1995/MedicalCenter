using FluentValidation;

namespace MedicalCenter.WebApi.Endpoints.Patients.Allergies;

/// <summary>
/// Validator for update allergy request.
/// </summary>
public class UpdateAllergyEndpointValidator : AbstractValidator<UpdateAllergyRequest>
{
    public UpdateAllergyEndpointValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("Patient ID is required.");

        RuleFor(x => x.AllergyId)
            .NotEmpty()
            .WithMessage("Allergy ID is required.");

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

