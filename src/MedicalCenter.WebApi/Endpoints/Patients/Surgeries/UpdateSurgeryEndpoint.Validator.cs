using FluentValidation;

namespace MedicalCenter.WebApi.Endpoints.Patients.Surgeries;

/// <summary>
/// Validator for update surgery request.
/// </summary>
public class UpdateSurgeryEndpointValidator : AbstractValidator<UpdateSurgeryRequest>
{
    public UpdateSurgeryEndpointValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("Patient ID is required.");

        RuleFor(x => x.SurgeryId)
            .NotEmpty()
            .WithMessage("Surgery ID is required.");

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

