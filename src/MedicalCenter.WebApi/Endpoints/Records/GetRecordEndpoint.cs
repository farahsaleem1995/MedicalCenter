using FastEndpoints;
using MedicalCenter.Core.Aggregates.MedicalRecord;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Records;

/// <summary>
/// Get specific medical record endpoint.
/// </summary>
public class GetRecordEndpoint(IMedicalRecordQueryService recordQueryService)
    : Endpoint<GetRecordRequest, GetRecordResponse>
{
    public override void Configure()
    {
        Get("/records/{recordId}");
        Group<RecordsGroup>();
        Policies(AuthorizationPolicies.CanViewRecords);
        Summary(s =>
        {
            s.Summary = "Get medical record";
            s.Description = "Gets a specific medical record. Practitioners can view all records.";
            s.Responses[200] = "Record retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - not authorized to view this record";
            s.Responses[404] = "Record not found";
        });
    }

    public override async Task HandleAsync(GetRecordRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            ThrowError("Invalid user authentication", 401);
            return;
        }

        var record = await recordQueryService.GetRecordByIdAsync(req.RecordId, ct);

        if (record == null)
        {
            ThrowError("Record not found", 404);
            return;
        }

        // Resource-based authorization: Only practitioner can view (or we can add CanViewAllPatients check)
        // For now, only practitioner can view
        if (record.PractitionerId != currentUserId)
        {
            ThrowError("You are not authorized to view this record", 403);
            return;
        }

        await Send.OkAsync(new GetRecordResponse
        {
            Id = record.Id,
            PatientId = record.PatientId,
            Patient = record.Patient != null ? new GetRecordResponse.PatientSummaryDto
            {
                Id = record.Patient.Id,
                FullName = record.Patient.FullName,
                Email = record.Patient.Email
            } : null,
            PractitionerId = record.PractitionerId,
            Practitioner = new GetRecordResponse.PractitionerDto
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
