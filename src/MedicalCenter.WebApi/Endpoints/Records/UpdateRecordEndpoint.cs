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
/// Update medical record endpoint.
/// </summary>
[Command]
public class UpdateRecordEndpoint(
    IRepository<MedicalRecord> recordRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext)
    : Endpoint<UpdateRecordRequest, UpdateRecordResponse>
{
    public override void Configure()
    {
        Put("/records/{recordId}");
        Group<RecordsGroup>();
        Policies(AuthorizationPolicies.CanModifyRecords);
        Summary(s =>
        {
            s.Summary = "Update medical record";
            s.Description = "Updates a medical record. Only the practitioner can modify the record.";
            s.Responses[200] = "Record updated successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - only practitioner can modify";
            s.Responses[404] = "Record not found";
        });
    }

    public override async Task HandleAsync(UpdateRecordRequest req, CancellationToken ct)
    {
        var currentUserId = userContext.UserId;

        var specification = new MedicalRecordByIdSpecification(req.RecordId);
        var record = await recordRepository.FirstOrDefaultAsync(specification, ct);

        if (record == null)
        {
            ThrowError("Record not found", 404);
            return;
        }

        // Domain enforces practitioner check, but we check here too for better error message
        if (record.PractitionerId != currentUserId)
        {
            ThrowError("Only the practitioner of the record can modify it", 403);
            return;
        }

        record.Update(currentUserId, req.Title, req.Content);
        await recordRepository.UpdateAsync(record, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await Send.OkAsync(new UpdateRecordResponse
        {
            Id = record.Id,
            PatientId = record.PatientId,
            PractitionerId = record.PractitionerId,
            Practitioner = new UpdateRecordResponse.PractitionerDto
            {
                FullName = record.Practitioner.FullName,
                Email = record.Practitioner.Email,
                Role = record.Practitioner.Role
            },
            RecordType = record.RecordType,
            Title = record.Title,
            Content = record.Content,
            Attachments = [.. record.Attachments.Select(a => new AttachmentDto
            {
                FileId = a.FileId,
                FileName = a.FileName,
                FileSize = a.FileSize,
                ContentType = a.ContentType,
                UploadedAt = a.UploadedAt
            })],
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt
        }, ct);
    }
}
