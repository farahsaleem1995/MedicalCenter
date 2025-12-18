using FastEndpoints;
using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.MedicalRecords.Specifications;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Records;

/// <summary>
/// Download file attachment endpoint.
/// </summary>
public class DownloadAttachmentEndpoint(
    IRepository<MedicalRecord> recordRepository,
    IFileStorageService fileStorageService)
    : Endpoint<DownloadAttachmentRequest>
{
    public override void Configure()
    {
        Get("/records/{recordId}/attachments/{attachmentId}/download");
        Group<RecordsGroup>();
        Policies(AuthorizationPolicies.RequirePatientOrPractitioner);
        Summary(s =>
        {
            s.Summary = "Download attachment";
            s.Description = "Downloads a file attachment from a medical record. Practitioners can download attachments from any patient's records. Patients can download attachments from their own records.";
            s.Responses[200] = "File downloaded successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - not authorized to access this attachment";
            s.Responses[404] = "Record or attachment not found";
        });
    }

    public override async Task HandleAsync(DownloadAttachmentRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            ThrowError("Invalid user authentication", 401);
            return;
        }

        var specification = new MedicalRecordByIdSpecification(req.RecordId);
        var record = await recordRepository.FirstOrDefaultAsync(specification, ct);

        if (record == null)
        {
            ThrowError("Record not found", 404);
            return;
        }

        // Find attachment in record
        var attachment = record.Attachments.FirstOrDefault(a => a.FileId == req.AttachmentId);
        if (attachment == null)
        {
            ThrowError("Attachment not found in this record", 404);
            return;
        }

        // Resource-based authorization
        var isPatient = User.IsInRole("Patient");

        // Patients can only access their own records
        if (isPatient && record.PatientId != currentUserId)
        {
            ThrowError("You are not authorized to access this attachment", 403);
            return;
        }

        // Practitioners (with RequirePatientOrPractitioner policy) can access any patient's records
        // No additional check needed - policy already ensures user is either Patient or Practitioner

        // Download file
        var downloadResult = await fileStorageService.DownloadFileAsync(req.AttachmentId, ct);
        if (downloadResult.IsFailure)
        {
            ThrowError($"Failed to download file: {downloadResult.Error!.Message}", 404);
            return;
        }

        await Send.StreamAsync(
            stream: downloadResult.Value!.FileStream,
            fileName: downloadResult.Value!.FileName,
            contentType: downloadResult.Value.ContentType,
            cancellation: ct);
    }
}
