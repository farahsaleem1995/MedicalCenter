using FastEndpoints;
using MedicalCenter.Core.Enums;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Authorization;
using MedicalCenter.WebApi.Extensions;

namespace MedicalCenter.WebApi.Endpoints.Admin;

/// <summary>
/// Admin endpoint to list users with optional filtering.
/// </summary>
public class ListUsersEndpoint(
    IUserQueryService userQueryService)
    : Endpoint<ListUsersRequest, ListUsersResponse>
{
    public override void Configure()
    {
        Get("/users");
        Group<AdminGroup>();
        Policies(AuthorizationPolicies.RequireAdmin);
        Summary(s =>
        {
            s.Summary = "List users";
            s.Description = "Allows system admin to list users with optional filtering by role and active status. Supports pagination.";
            s.Params["pageNumber"] = "Page number (default: 1, minimum: 1)";
            s.Params["pageSize"] = "Number of items per page (default: 10, minimum: 1, maximum: 100)";
            s.Params["role"] = "Optional: Filter by user role (Doctor, HealthcareStaff, LabUser, ImagingUser, Patient, SystemAdmin)";
            s.Params["isActive"] = "Optional: Filter by active status (true, false)";
            s.Responses[200] = "Users retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - Admin access required";
        });
    }

    public override async Task HandleAsync(ListUsersRequest req, CancellationToken ct)
    {
        UserRole? role = null;
        if (!string.IsNullOrWhiteSpace(req.Role))
        {
            if (Enum.TryParse<UserRole>(req.Role, true, out var parsedRole))
            {
                role = parsedRole;
            }
        }

        bool? isActive = req.IsActive;

        // Use admin method to ignore query filters (include deactivated users)
        var paginatedResult = await userQueryService.ListUsersAdminPaginatedAsync(
            req.PageNumber ?? 1,
            req.PageSize ?? 10,
            role,
            isActive,
            ct);

        Response = new ListUsersResponse
        {
            Items = paginatedResult.Items.Select(GetUserResponse.FromUser).ToList(),
            Metadata = paginatedResult.Metadata
        };
    }
}

