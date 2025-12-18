using FastEndpoints;
using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.MedicalRecords.Enums;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Queries;
using MedicalCenter.Core.Authorization;
using MedicalCenter.WebApi.Endpoints.Records;

namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Get current patient's specific medical record endpoint.
/// </summary>
public class GetSelfRecordEndpoint(IMedicalRecordQueryService recordQueryService)
    : Endpoint<GetSelfRecordRequest, GetSelfRecordResponse>
{
    public override void Configure()
    {
        Get("/patients/self/records/{recordId}");
        Group<PatientsGroup>();
        Policies(AuthorizationPolicies.RequirePatient);
        Summary(s =>
        {
            s.Summary = "Get patient's specific medical record";
            s.Description = "Returns a specific medical record for the authenticated patient.";
            s.Responses[200] = "Record retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - record does not belong to patient";
            s.Responses[404] = "Record not found";
        });
    }

    public override async Task HandleAsync(GetSelfRecordRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var patientId))
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

        // Resource-based authorization: Verify record belongs to patient
        if (record.PatientId != patientId)
        {
            ThrowError("You are not authorized to view this record", 403);
            return;
        }

        await Send.OkAsync(new GetSelfRecordResponse
        {
            Id = record.Id,
            RecordType = record.RecordType,
            Title = record.Title,
            Content = record.Content,
            Practitioner = new GetSelfRecordResponse.PractitionerDto
            {
                FullName = record.Practitioner.FullName,
                Email = record.Practitioner.Email,
                Role = record.Practitioner.Role
            },
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
