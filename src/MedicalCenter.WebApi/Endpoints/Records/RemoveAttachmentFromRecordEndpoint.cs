using FastEndpoints;
using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.MedicalRecords.Specifications;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.Authorization;
using MedicalCenter.WebApi.Attributes;

namespace MedicalCenter.WebApi.Endpoints.Records;

/// <summary>
/// Remove attachment from medical record endpoint.
/// </summary>
[ActionLog("Attachment removed from medical record")]
public class RemoveAttachmentFromRecordEndpoint(
    IRepository<MedicalRecord> recordRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext)
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
