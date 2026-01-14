using FastEndpoints;
using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Core.Queries;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Practitioners;

/// <summary>
/// List current practitioner's medical records endpoint.
/// </summary>
public class ListSelfRecordsEndpoint(
    IMedicalRecordQueryService medicalRecordQueryService,
    IUserContext userContext)
    : Endpoint<ListSelfRecordsRequest, ListSelfRecordsResponse>
{
    public override void Configure()
    {
        Get("/self/records");
        Group<PractitionersGroup>();
        Policies(AuthorizationPolicies.CanViewRecords);
        Summary(s =>
        {
            s.Summary = "List practitioner's records";
            s.Description = "Lists medical records created by the authenticated practitioner. Supports optional filtering by patient, record type, and date range. Supports pagination.";
            s.Params["pageNumber"] = "Page number (default: 1, minimum: 1)";
            s.Params["pageSize"] = "Number of items per page (default: 10, minimum: 1, maximum: 100)";
            s.Params["patientId"] = "Optional: Filter by patient ID";
            s.Params["recordType"] = "Optional: Filter by record type";
            s.Params["dateFrom"] = "Optional: Filter records from this date";
            s.Params["dateTo"] = "Optional: Filter records to this date";
            s.Responses[200] = "Records retrieved successfully";
            s.Responses[401] = "Unauthorized";
        });
    }

    public override async Task HandleAsync(ListSelfRecordsRequest req, CancellationToken ct)
    {
        var practitionerId = userContext.UserId;

        var records = await medicalRecordQueryService.ListRecordsAsync(
            new PaginationQuery<ListRecordsQuery>(req.PageNumber ?? 1, req.PageSize ?? 10)
            {
                Criteria = new ListRecordsQuery
                {
                    PractitionerId = practitionerId,
                    PatientId = req.PatientId,
                    RecordType = req.RecordType,
                    DateFrom = req.DateFrom,
                    DateTo = req.DateTo
                }
            }, ct);

        await Send.OkAsync(new ListSelfRecordsResponse
        {
            Items = records.Items.Select(PractitionerRecordSummaryDto.FromMedicalRecord).ToList(),
            Metadata = records.Metadata
        }, ct);
    }
}


