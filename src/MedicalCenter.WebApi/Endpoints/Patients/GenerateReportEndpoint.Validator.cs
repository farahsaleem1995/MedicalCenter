using FluentValidation;
using FastEndpoints;

namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Validator for GenerateReportRequest.
/// </summary>
public class GenerateReportValidator : Validator<GenerateReportRequest>
{
    public GenerateReportValidator()
    {
        RuleFor(x => x.DateFrom)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.DateFrom.HasValue)
            .WithMessage("DateFrom cannot be in the future.");

        RuleFor(x => x.DateTo)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.DateTo.HasValue)
            .WithMessage("DateTo cannot be in the future.");

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithMessage("DateTo must be greater than or equal to From.");
    }
}

