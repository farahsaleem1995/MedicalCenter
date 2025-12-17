using FastEndpoints;
using FluentValidation;

namespace MedicalCenter.WebApi.Endpoints.Records;

public class UpdateRecordEndpointValidator : Validator<UpdateRecordRequest>
{
    public UpdateRecordEndpointValidator()
    {
        RuleFor(x => x.RecordId)
            .NotEmpty()
            .WithMessage("Record ID is required.");

        RuleFor(x => x.Title)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Title cannot exceed 500 characters.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .When(x => string.IsNullOrEmpty(x.Title))
            .WithMessage("Either title or content must be provided.");
    }
}
