using FluentValidation;

namespace MedicalCenter.WebApi.Endpoints.Patients.ChronicDiseases;

/// <summary>
/// Validator for update chronic disease request.
/// </summary>
public class UpdateChronicDiseaseEndpointValidator : AbstractValidator<UpdateChronicDiseaseRequest>
{
    public UpdateChronicDiseaseEndpointValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("Patient ID is required.");

        RuleFor(x => x.ChronicDiseaseId)
            .NotEmpty()
            .WithMessage("Chronic disease ID is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 1000 characters.");
    }
}

