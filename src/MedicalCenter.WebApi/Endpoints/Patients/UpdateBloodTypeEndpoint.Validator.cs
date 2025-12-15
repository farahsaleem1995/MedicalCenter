using FastEndpoints;
using FluentValidation;

namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Validator for update blood type request.
/// </summary>
public class UpdateBloodTypeEndpointValidator : Validator<UpdateBloodTypeRequest>
{
    public UpdateBloodTypeEndpointValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("Patient ID is required.");

        // Either both ABO and Rh must be provided, or both must be null (to clear blood type)
        RuleFor(x => x)
            .Must(x => (x.ABO.HasValue && x.Rh.HasValue) || (!x.ABO.HasValue && !x.Rh.HasValue))
            .WithMessage("Both ABO and Rh must be provided together, or both must be omitted to clear the blood type.");

        RuleFor(x => x.ABO)
            .IsInEnum()
            .When(x => x.ABO.HasValue)
            .WithMessage("Invalid ABO blood type.");

        RuleFor(x => x.Rh)
            .IsInEnum()
            .When(x => x.Rh.HasValue)
            .WithMessage("Invalid Rh factor.");
    }
}
