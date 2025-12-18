using FastEndpoints;
using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.MedicalRecords.Specifications;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Get current patient's own medical records endpoint.
/// </summary>
public class GetSelfRecordsEndpoint(IRepository<MedicalRecord> recordRepository)
    : Endpoint<GetSelfRecordsRequest, GetSelfRecordsResponse>
{
    public override void Configure()
    {
        Get("/patients/self/records");
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
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var patientId))
        {
            ThrowError("Invalid user authentication", 401);
            return;
        }

        var specification = new MedicalRecordsByPatientSpecification(patientId);
        var records = await recordRepository.ListAsync(specification, ct);

        // Apply filters
        var filteredRecords = records.AsEnumerable().AsQueryable();
        if (req.RecordType.HasValue)
        {
            filteredRecords = filteredRecords.Where(r => r.RecordType == req.RecordType.Value);
        }
        if (req.DateFrom.HasValue)
        {
            filteredRecords = filteredRecords.Where(r => r.CreatedAt >= req.DateFrom.Value);
        }
        if (req.DateTo.HasValue)
        {
            filteredRecords = filteredRecords.Where(r => r.CreatedAt <= req.DateTo.Value);
        }

        var recordsList = filteredRecords.ToList();

        await Send.OkAsync(new GetSelfRecordsResponse
        {
            Records = [.. recordsList.Select(r => new PatientRecordSummaryDto
            {
                Id = r.Id,
                RecordType = r.RecordType,
                Title = r.Title,
                CreatedAt = r.CreatedAt,
                AttachmentCount = r.Attachments.Count
            })]
        }, ct);
    }
}
