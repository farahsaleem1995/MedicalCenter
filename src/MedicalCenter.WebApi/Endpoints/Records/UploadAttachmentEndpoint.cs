using FastEndpoints;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.Authorization;
using MedicalCenter.Infrastructure.Options;
using Microsoft.Extensions.Options;
using MedicalCenter.WebApi.Attributes;

namespace MedicalCenter.WebApi.Endpoints.Records;

/// <summary>
/// Upload file attachment endpoint.
/// </summary>
[ActionLog("File attachment uploaded")]
public class UploadAttachmentEndpoint(
    IFileStorageService fileStorageService,
    IOptions<FileStorageOptions> fileStorageOptions)
    : Endpoint<UploadAttachmentRequest, UploadAttachmentResponse>
{
    public override void Configure()
    {
        Post("/records/attachments/upload");
        Group<RecordsGroup>();
        Policies(AuthorizationPolicies.CanModifyRecords);
        AllowFileUploads();
        Summary(s =>
        {
            s.Summary = "Upload file attachment";
            s.Description = "Uploads a file attachment that can be referenced when creating medical records. Returns attachment metadata including fileId.";
            s.Responses[200] = "File uploaded successfully";
            s.Responses[400] = "Invalid file or validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - insufficient permissions";
        });
    }

    public override async Task HandleAsync(UploadAttachmentRequest req, CancellationToken ct)
    {
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

        await Send.OkAsync(new UploadAttachmentResponse
        {
            AttachmentId = uploadResult.Value!.FileId,
            FileName = uploadResult.Value.FileName,
            FileSize = uploadResult.Value.FileSize,
            ContentType = uploadResult.Value.ContentType,
            UploadedAt = uploadResult.Value.UploadedAt
        }, ct);
    }
}
