using FastEndpoints;
using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.MedicalRecords.Enums;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Queries;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.Authorization;
using MedicalCenter.WebApi.Attributes;

namespace MedicalCenter.WebApi.Endpoints.Records;

/// <summary>
/// Get specific medical record endpoint.
/// </summary>
[ActionLog("Medical record viewed")]
public class GetRecordEndpoint(
    IMedicalRecordQueryService recordQueryService,
    IUserContext userContext)
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
        var currentUserId = userContext.UserId;

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
