using FastEndpoints;
using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Core.Queries;
using MedicalCenter.Core.Authorization;

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
        
        var query = new PaginationQuery<ListRecordsQuery>(req.PageNumber ?? 1, req.PageSize ?? 10)
        {
            Criteria = new ListRecordsQuery
            {
                PractitionerId = req.PractitionerId,
                PatientId = req.PatientId,
                RecordType = req.RecordType,
                DateFrom = req.DateFrom,
                DateTo = req.DateTo,
            }
        };

        var paginatedResult = await recordQueryService.ListRecordsAsync(query, ct);

        await Send.OkAsync(new ListRecordsResponse
        {
            Items = paginatedResult.Items.Select(RecordSummaryDto.FromMedicalRecord).ToList(),
            Metadata = paginatedResult.Metadata
        }, ct);
    }
}
