using FastEndpoints;
using FluentValidation;
using MedicalCenter.Core.Services;

namespace MedicalCenter.WebApi.Endpoints.Patients.ChronicDiseases;

/// <summary>
/// Validator for create chronic disease request.
/// </summary>
public class CreateChronicDiseaseEndpointValidator : Validator<CreateChronicDiseaseRequest>
{
    public CreateChronicDiseaseEndpointValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("Patient ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Chronic disease name is required.")
            .MaximumLength(200)
            .WithMessage("Chronic disease name cannot exceed 200 characters.");

        RuleFor(x => x.DiagnosisDate)
            .NotEmpty()
            .WithMessage("Diagnosis date is required.")
            .LessThanOrEqualTo(_ => dateTimeProvider.Now)
            .WithMessage("Diagnosis date cannot be in the future.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 1000 characters.");
    }
}

