using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Queries;
using MedicalCenter.WebApi.Extensions;
using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.WebApi.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Admin;

/// <summary>
/// Admin endpoint to list users with optional filtering.
/// </summary>
public class ListUsersEndpoint(
    IUserQueryService userQueryService,
    IAuthorizationService authorizationService)
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
            s.Description = "Allows system admin to list users with optional filtering by role, active status, and national ID. Supports pagination and sorting. All SystemAdmin users can view SystemAdmin users in the list. Filtering by SystemAdmin role requires Super Administrator privileges.";
            s.Params["pageNumber"] = "Page number (default: 1, minimum: 1)";
            s.Params["pageSize"] = "Number of items per page (default: 10, minimum: 1, maximum: 100)";
            s.Params["role"] = "Optional: Filter by user role (Doctor, HealthcareStaff, LabUser, ImagingUser, Patient, SystemAdmin)";
            s.Params["isActive"] = "Optional: Filter by active status (true, false)";
            s.Params["nationalId"] = "Optional: Filter by national ID (partial match, case-insensitive)";
            s.Params["sortBy"] = "Optional: Sort field (FullName, Email, Role, CreatedAt, NationalId). Default: FullName";
            s.Params["sortDirection"] = "Optional: Sort direction (Asc, Desc). Default: Asc";
            s.Responses[200] = "Users retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - Admin access required or insufficient privileges for SystemAdmin filtering";
        });
    }

    public override async Task HandleAsync(ListUsersRequest req, CancellationToken ct)
    {
        // Business rule: Filtering by SystemAdmin role requires Super Admin privileges
        if (req.Role == UserRole.SystemAdmin)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(
                User, 
                AuthorizationPolicies.CanManageAdmins);
            
            if (!authorizationResult.Succeeded)
            {
                ThrowError("Only Super Administrators can filter by SystemAdmin role.", 403);
                return;
            }
        }

        var query = new PaginationQuery<ListUsersQuery>(req.PageNumber ?? 1, req.PageSize ?? 10)
        {
            Criteria = new ListUsersQuery
            {
                Role = req.Role,
                IsActive = req.IsActive,
                NationalId = string.IsNullOrWhiteSpace(req.NationalId) ? null : req.NationalId.Trim(),
                SortBy = req.SortBy ?? ListUsersSortBy.FullName,
                SortDirection = req.SortDirection ?? SortDirection.Asc
            }
        };
        var paginatedResult = await userQueryService.ListUsersPaginatedAsync(query, ct);

        // All SystemAdmin users can view SystemAdmin users in the list
        // Only Super Admins (with CanManageAdmins policy) can modify them
        await Send.OkAsync(new ListUsersResponse
        {
            Items = paginatedResult.Items.Select(GetUserResponse.FromUser).ToList(),
            Metadata = paginatedResult.Metadata
        }, ct);
    }
}

