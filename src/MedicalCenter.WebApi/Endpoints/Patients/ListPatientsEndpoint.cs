using FastEndpoints;
using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Core.Queries;
using MedicalCenter.WebApi.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// List all patients endpoint.
/// Practitioners can view all patients with optional filtering.
/// </summary>
public class ListPatientsEndpoint(IPatientQueryService patientQueryService)
    : Endpoint<ListPatientsRequest, ListPatientsResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<PatientsGroup>();
        Policies(AuthorizationPolicies.CanViewPatients);
        Summary(s =>
        {
            s.Summary = "List patients";
            s.Description = "Lists all patients. Practitioners can view all patients. Supports filtering by search term and date of birth range. Supports pagination. Returns active patients only.";
            s.Params["pageNumber"] = "Page number (default: 1, minimum: 1)";
            s.Params["pageSize"] = "Number of items per page (default: 10, minimum: 1, maximum: 100)";
            s.Params["searchTerm"] = "Optional: Search in full name, email, or national ID";
            s.Params["dateOfBirthFrom"] = "Optional: Filter patients born from this date";
            s.Params["dateOfBirthTo"] = "Optional: Filter patients born to this date";
            s.Responses[200] = "Patients retrieved successfully";
            s.Responses[401] = "Unauthorized";
        });
    }

    public override async Task HandleAsync(ListPatientsRequest req, CancellationToken ct)
    {
        var query = new PaginationQuery<ListPatientsQuery>(req.PageNumber ?? 1, req.PageSize ?? 10)
        {
            Criteria = new ListPatientsQuery
            {
                SearchTerm = req.SearchTerm,
                DateOfBirthFrom = req.DateOfBirthFrom,
                DateOfBirthTo = req.DateOfBirthTo
            }
        };

        var paginatedResult = await patientQueryService.ListPatientsAsync(query, ct);

        await Send.OkAsync(new ListPatientsResponse
        {
            Items = paginatedResult.Items.Select(PatientSummaryDto.FromPatient).ToList(),
            Metadata = paginatedResult.Metadata
        }, ct);
    }
}

