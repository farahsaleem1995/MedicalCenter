using FastEndpoints;
using FluentValidation;
using MedicalCenter.Core.Enums;
using MedicalCenter.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace MedicalCenter.WebApi.Endpoints.Records;

public class CreateRecordEndpointValidator : Validator<CreateRecordRequest>
{
    private readonly FileStorageOptions _fileStorageOptions;

    public CreateRecordEndpointValidator(IOptions<FileStorageOptions> fileStorageOptions)
    {
        _fileStorageOptions = fileStorageOptions.Value;
        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("Patient ID is required.");

        RuleFor(x => x.RecordType)
            .IsInEnum()
            .WithMessage("Invalid record type.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(500)
            .WithMessage("Title cannot exceed 500 characters.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required.");

        RuleFor(x => x.AttachmentIds)
            .Must(ids => ids == null || ids.Count <= _fileStorageOptions.MaxAttachmentsPerRecord)
            .WithMessage($"Maximum {_fileStorageOptions.MaxAttachmentsPerRecord} attachments allowed per record.")
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("Invalid attachment ID.");
    }
}
