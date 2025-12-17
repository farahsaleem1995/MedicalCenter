using FastEndpoints;
using MedicalCenter.Core.Aggregates.MedicalRecord;
using MedicalCenter.Core.Enums;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Records;

/// <summary>
/// List all medical records endpoint.
/// Practitioners can view all records with optional filtering.
/// </summary>
public class ListRecordsEndpoint(IMedicalRecordQueryService recordQueryService)
    : Endpoint<ListRecordsRequest, ListRecordsResponse>
{
    public override void Configure()
    {
        Get("/records");
        Group<RecordsGroup>();
        Policies(AuthorizationPolicies.CanViewRecords);
        Summary(s =>
        {
            s.Summary = "List medical records";
            s.Description = "Lists all medical records. Practitioners can view all records. Supports filtering by practitioner, patient, record type, and date range. Supports pagination.";
            s.Params["pageNumber"] = "Page number (default: 1, minimum: 1)";
            s.Params["pageSize"] = "Number of items per page (default: 10, minimum: 1, maximum: 100)";
            s.Params["practitionerId"] = "Optional: Filter by practitioner ID";
            s.Params["patientId"] = "Optional: Filter by patient ID";
            s.Params["recordType"] = "Optional: Filter by record type";
            s.Params["dateFrom"] = "Optional: Filter records from this date";
            s.Params["dateTo"] = "Optional: Filter records to this date";
            s.Responses[200] = "Records retrieved successfully";
            s.Responses[401] = "Unauthorized";
        });
    }

    public override async Task HandleAsync(ListRecordsRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out _))
        {
            ThrowError("Invalid user authentication", 401);
            return;
        }

        // Use query service for optimized query with includes
        // Practitioners can view all records, with optional filters
        var pageNumber = req.PageNumber ?? 1;
        var pageSize = req.PageSize ?? 10;
        var paginatedResult = await recordQueryService.ListRecordsAsync(
            pageNumber,
            pageSize,
            req.PractitionerId,
            req.PatientId,
            req.RecordType,
            req.DateFrom,
            req.DateTo,
            ct);

        // Map to DTOs
        var recordDtos = paginatedResult.Items.Select(r => new RecordSummaryDto
        {
            Id = r.Id,
            PatientId = r.PatientId,
            Patient = r.Patient != null ? new RecordSummaryDto.PatientSummaryDto
            {
                Id = r.Patient.Id,
                FullName = r.Patient.FullName,
                Email = r.Patient.Email
            } : null,
            PractitionerId = r.PractitionerId,
            Practitioner = new RecordSummaryDto.PractitionerDto
            {
                FullName = r.Practitioner.FullName,
                Email = r.Practitioner.Email,
                Role = r.Practitioner.Role
            },
            RecordType = r.RecordType,
            Title = r.Title,
            CreatedAt = r.CreatedAt,
            AttachmentCount = r.Attachments.Count
        }).ToList();

        await Send.OkAsync(new ListRecordsResponse
        {
            Items = recordDtos,
            Metadata = paginatedResult.Metadata
        }, ct);
    }
}
