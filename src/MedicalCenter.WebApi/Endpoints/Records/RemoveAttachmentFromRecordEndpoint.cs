using FastEndpoints;
using MedicalCenter.Core.Aggregates.MedicalRecord;
using MedicalCenter.Core.Aggregates.MedicalRecord.Specifications;
using MedicalCenter.Core.Common;
using MedicalCenter.Infrastructure.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Records;

/// <summary>
/// Remove attachment from medical record endpoint.
/// </summary>
public class RemoveAttachmentFromRecordEndpoint(
    IRepository<MedicalRecord> recordRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<RemoveAttachmentFromRecordRequest>
{
    public override void Configure()
    {
        Delete("/records/{recordId}/attachments/{attachmentId}");
        Group<RecordsGroup>();
        Policies(AuthorizationPolicies.CanModifyRecords);
        Summary(s =>
        {
            s.Summary = "Remove attachment from medical record";
            s.Description = "Removes an attachment from an existing medical record. Only the practitioner of the record can remove attachments. The file itself is not deleted from storage.";
            s.Responses[204] = "Attachment removed successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - only practitioner can remove attachments";
            s.Responses[404] = "Record or attachment not found";
        });
    }

    public override async Task HandleAsync(RemoveAttachmentFromRecordRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            ThrowError("Invalid user authentication", 401);
            return;
        }

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
            ThrowError("Only the practitioner of the record can remove attachments", 403);
            return;
        }

        // Verify attachment exists in record
        if (!record.Attachments.Any(a => a.FileId == req.AttachmentId))
        {
            ThrowError("Attachment not found in this record", 404);
            return;
        }

        // Remove attachment from record (domain enforces business rules)
        record.RemoveAttachment(currentUserId, req.AttachmentId);

        await recordRepository.UpdateAsync(record, ct);
        await unitOfWork.SaveChangesAsync(ct);

        // Note: File is not deleted from storage - only the reference is removed from the record
        // This allows for potential recovery or audit purposes

        await Send.NoContentAsync(ct);
    }
}
