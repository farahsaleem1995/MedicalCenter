using FastEndpoints;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Authorization;
using MedicalCenter.Core.Queries;
using MedicalCenter.Core.Primitives.Pagination;

namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Get current patient's own medical records endpoint.
/// </summary>
public class GetSelfRecordsEndpoint(
    IMedicalRecordQueryService medicalRecordQueryService,
    IUserContext userContext)
    : Endpoint<GetSelfRecordsRequest, GetSelfRecordsResponse>
{
    public override void Configure()
    {
        Get("/self/records");
        Group<PatientsGroup>();
        Policies(AuthorizationPolicies.RequirePatient);
        Summary(s =>
        {
            s.Summary = "Get patient's own medical records";
            s.Description = "Returns all medical records for the authenticated patient. Supports filtering by record type and date range.";
            s.Responses[200] = "Records retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(GetSelfRecordsRequest req, CancellationToken ct)
    {
        var patientId = userContext.UserId;

        var records = await medicalRecordQueryService.ListRecordsAsync(
            new PaginationQuery<ListRecordsQuery>(req.PageNumber ?? 1, req.PageSize ?? 10)
            {
                Criteria = new ListRecordsQuery
                {
                    PatientId = patientId,
                    RecordType = req.RecordType,
                    DateFrom = req.DateFrom,
                    DateTo = req.DateTo
                }
            }, ct);

        await Send.OkAsync(new GetSelfRecordsResponse
        {
            Records = [.. records.Items.Select(PatientRecordSummaryDto.FromMedicalRecord)],
            Metadata = records.Metadata
        }, ct);
    }
}
