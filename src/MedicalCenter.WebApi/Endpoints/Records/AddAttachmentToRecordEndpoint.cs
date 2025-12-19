using FastEndpoints;
using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.MedicalRecords.Specifications;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.Authorization;
using MedicalCenter.Infrastructure.Options;
using Microsoft.Extensions.Options;
using MedicalCenter.WebApi.Attributes;

namespace MedicalCenter.WebApi.Endpoints.Records;

/// <summary>
/// Add attachment to existing medical record endpoint.
/// </summary>
[ActionLog("Attachment added to medical record")]
public class AddAttachmentToRecordEndpoint(
    IRepository<MedicalRecord> recordRepository,
    IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork,
    IOptions<FileStorageOptions> fileStorageOptions,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext)
    : Endpoint<AddAttachmentToRecordRequest, AddAttachmentToRecordResponse>
{
    public override void Configure()
    {
        Post("/records/{recordId}/attachments");
        Group<RecordsGroup>();
        Policies(AuthorizationPolicies.CanModifyRecords);
        AllowFileUploads();
        Summary(s =>
        {
            s.Summary = "Add attachment to medical record";
            s.Description = "Uploads a file attachment and adds it to an existing medical record. Only the practitioner of the record can add attachments.";
            s.Responses[200] = "Attachment added successfully";
            s.Responses[400] = "Invalid file, validation error, or business rule violation";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - only practitioner can add attachments";
            s.Responses[404] = "Record not found";
        });
    }

    public override async Task HandleAsync(AddAttachmentToRecordRequest req, CancellationToken ct)
    {
        var currentUserId = userContext.UserId;

        // Find the record
        var specification = new MedicalRecordByIdSpecification(req.RecordId);
        var record = await recordRepository.FirstOrDefaultAsync(specification, ct);

        if (record == null)
        {
            ThrowError("Record not found", 404);
            return;
        }

        // Verify practitioner
        if (record.PractitionerId != currentUserId)
        {
            ThrowError("Only the practitioner of the record can add attachments", 403);
            return;
        }

        // Validate file
        if (req.File == null || req.File.Length == 0)
        {
            AddError("File is required");
            ThrowIfAnyErrors();
            return;
        }

        var options = fileStorageOptions.Value;

        // Validate file size
        if (req.File.Length > options.MaxFileSizeBytes)
        {
            AddError($"File size exceeds maximum allowed size of {options.MaxFileSizeBytes / (1024 * 1024)}MB");
            ThrowIfAnyErrors();
            return;
        }

        // Validate content type
        if (!options.AllowedContentTypes.Contains(req.File.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            AddError($"Content type '{req.File.ContentType}' is not allowed. Allowed types: {string.Join(", ", options.AllowedContentTypes)}");
            ThrowIfAnyErrors();
            return;
        }

        // Check attachment limit
        if (record.Attachments.Count >= options.MaxAttachmentsPerRecord)
        {
            AddError($"Maximum {options.MaxAttachmentsPerRecord} attachments allowed per record");
            ThrowIfAnyErrors();
            return;
        }

        // Upload file
        var uploadResult = await fileStorageService.UploadFileAsync(
            req.File.OpenReadStream(),
            req.File.FileName,
            req.File.ContentType,
            ct);

        if (uploadResult.IsFailure)
        {
            AddError(uploadResult.Error!.Message);
            ThrowIfAnyErrors();
            return;
        }

        // Create attachment value object
        var attachment = Attachment.Create(
            uploadResult.Value!.FileId,
            uploadResult.Value.FileName,
            uploadResult.Value.FileSize,
            uploadResult.Value.ContentType,
            dateTimeProvider.Now);

        // Add attachment to record (domain enforces business rules)
        record.AddAttachment(currentUserId, attachment);

        await recordRepository.UpdateAsync(record, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await Send.OkAsync(new AddAttachmentToRecordResponse
        {
            AttachmentId = attachment.FileId,
            FileName = attachment.FileName,
            FileSize = attachment.FileSize,
            ContentType = attachment.ContentType,
            UploadedAt = attachment.UploadedAt
        }, ct);
    }
}
