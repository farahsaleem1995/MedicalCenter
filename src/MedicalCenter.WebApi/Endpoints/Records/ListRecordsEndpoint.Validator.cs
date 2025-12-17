using FastEndpoints;
using FluentValidation;

namespace MedicalCenter.WebApi.Endpoints.Records;

public class ListRecordsEndpointValidator : Validator<ListRecordsRequest>
{
    public ListRecordsEndpointValidator()
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

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .WithMessage("DateTo must be greater than or equal to DateFrom")
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);
    }
}
