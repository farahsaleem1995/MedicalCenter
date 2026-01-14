using FastEndpoints;
using FluentValidation;

namespace MedicalCenter.WebApi.Endpoints.Patients;

public class ListPatientsEndpointValidator : Validator<ListPatientsRequest>
{
    public ListPatientsEndpointValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0")
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100")
            .When(x => x.PageSize.HasValue);

        RuleFor(x => x.DateOfBirthTo)
            .GreaterThanOrEqualTo(x => x.DateOfBirthFrom)
            .WithMessage("DateOfBirthTo must be greater than or equal to DateOfBirthFrom")
            .When(x => x.DateOfBirthFrom.HasValue && x.DateOfBirthTo.HasValue);
    }
}

