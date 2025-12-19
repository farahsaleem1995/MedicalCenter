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
/// Delete medical record endpoint (soft delete).
/// </summary>
[Command]
public class DeleteRecordEndpoint(
    IRepository<MedicalRecord> recordRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext)
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
        var currentUserId = userContext.UserId;

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
