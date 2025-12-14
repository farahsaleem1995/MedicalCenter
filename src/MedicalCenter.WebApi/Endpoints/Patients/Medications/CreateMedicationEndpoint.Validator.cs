using FluentValidation;

namespace MedicalCenter.WebApi.Endpoints.Patients.Medications;

/// <summary>
/// Validator for create medication request.
/// </summary>
public class CreateMedicationEndpointValidator : AbstractValidator<CreateMedicationRequest>
{
    public CreateMedicationEndpointValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("Patient ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Medication name is required.")
            .MaximumLength(200)
            .WithMessage("Medication name cannot exceed 200 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date cannot be before start date.");

        RuleFor(x => x.Dosage)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Dosage))
            .WithMessage("Dosage cannot exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 1000 characters.");
    }
}

