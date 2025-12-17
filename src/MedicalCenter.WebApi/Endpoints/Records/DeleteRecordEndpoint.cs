using FastEndpoints;
using MedicalCenter.Core.Aggregates.MedicalRecord;
using MedicalCenter.Core.Aggregates.MedicalRecord.Specifications;
using MedicalCenter.Core.Common;
using MedicalCenter.Infrastructure.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Records;

/// <summary>
/// Delete medical record endpoint (soft delete).
/// </summary>
public class DeleteRecordEndpoint(
    IRepository<MedicalRecord> recordRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<DeleteRecordRequest>
{
    public override void Configure()
    {
        Delete("/records/{recordId}");
        Group<RecordsGroup>();
        Policies(AuthorizationPolicies.CanModifyRecords);
        Summary(s =>
        {
            s.Summary = "Delete medical record";
            s.Description = "Soft deletes a medical record. Only the practitioner can delete the record.";
            s.Responses[204] = "Record deleted successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - only practitioner can delete";
            s.Responses[404] = "Record not found";
        });
    }

    public override async Task HandleAsync(DeleteRecordRequest req, CancellationToken ct)
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

        // Domain enforces practitioner check
        if (record.PractitionerId != currentUserId)
        {
            ThrowError("Only the practitioner of the record can delete it", 403);
            return;
        }

        record.Delete(currentUserId);
        await recordRepository.UpdateAsync(record, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
