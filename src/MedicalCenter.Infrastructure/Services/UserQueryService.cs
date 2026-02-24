using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Core.Queries;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Infrastructure.Identity;
using MedicalCenter.Infrastructure.Extensions;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of IUserQueryService for querying user entities.
/// Queries start from ASP.NET Core Identity's ApplicationUser to access all user types
/// (Patient, Doctor, HealthcareStaff, Laboratory, ImagingCenter, and SystemAdmin).
/// Includes related domain entities and roles for comprehensive user data retrieval.
/// </summary>
public class UserQueryService(UserManager<ApplicationUser> userManager)
    : IUserQueryService
{
    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await QueryUsers(null, null, null)
            .Where(u => u.Id == id)
            .Select(u => MapToDomainUser(u))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaginatedList<User>> ListUsersPaginatedAsync(
        PaginationQuery<ListUsersQuery> query,
        CancellationToken cancellationToken = default)
    {
        var criteria = query.Criteria;
        var dbQuery = QueryUsers(criteria?.Role, criteria?.IsActive, criteria?.NationalId);
        dbQuery = ApplySorting(dbQuery, criteria?.SortBy ?? ListUsersSortBy.FullName, criteria?.SortDirection ?? SortDirection.Asc);
        return await dbQuery
            .Select(u => MapToDomainUser(u))
            .ToPaginatedListAsync(query.PageNumber, query.PageSize, cancellationToken);
    }

    private IQueryable<ApplicationUser> QueryUsers(UserRole? role, bool? isActive, string? nationalId)
    {
        IQueryable<ApplicationUser> query = userManager.Users
            .IgnoreQueryFilters()
            .IncludeDomainEntities()
            .IncludeRoles();

        query = role.HasValue ? query.WhereInRole(role.Value) : query;

        query = isActive.HasValue
            ? (isActive.Value ? query.WhereActive() : query.WhereInactive())
            : query;

        if (!string.IsNullOrWhiteSpace(nationalId))
            query = query.WhereNationalIdContains(nationalId);

        return query;
    }

    private static IQueryable<ApplicationUser> ApplySorting(
        IQueryable<ApplicationUser> query,
        ListUsersSortBy sortBy,
        SortDirection sortDirection)
    {
        var orderedQuery = sortBy switch
        {
            ListUsersSortBy.FullName => sortDirection == SortDirection.Asc
                ? query.OrderBy(u => u.Patient != null ? u.Patient.FullName : u.Doctor != null ? u.Doctor.FullName : u.HealthcareStaff != null ? u.HealthcareStaff.FullName : u.Laboratory != null ? u.Laboratory.FullName : u.ImagingCenter != null ? u.ImagingCenter.FullName : u.SystemAdmin != null ? u.SystemAdmin.FullName : "")
                : query.OrderByDescending(u => u.Patient != null ? u.Patient.FullName : u.Doctor != null ? u.Doctor.FullName : u.HealthcareStaff != null ? u.HealthcareStaff.FullName : u.Laboratory != null ? u.Laboratory.FullName : u.ImagingCenter != null ? u.ImagingCenter.FullName : u.SystemAdmin != null ? u.SystemAdmin.FullName : ""),
            ListUsersSortBy.Email => sortDirection == SortDirection.Asc
                ? query.OrderBy(u => u.Patient != null ? u.Patient.Email : u.Doctor != null ? u.Doctor.Email : u.HealthcareStaff != null ? u.HealthcareStaff.Email : u.Laboratory != null ? u.Laboratory.Email : u.ImagingCenter != null ? u.ImagingCenter.Email : u.SystemAdmin != null ? u.SystemAdmin.Email : "")
                : query.OrderByDescending(u => u.Patient != null ? u.Patient.Email : u.Doctor != null ? u.Doctor.Email : u.HealthcareStaff != null ? u.HealthcareStaff.Email : u.Laboratory != null ? u.Laboratory.Email : u.ImagingCenter != null ? u.ImagingCenter.Email : u.SystemAdmin != null ? u.SystemAdmin.Email : ""),
            ListUsersSortBy.Role => sortDirection == SortDirection.Asc
                ? query.OrderBy(u => u.Patient != null ? u.Patient.Role.ToString() : u.Doctor != null ? u.Doctor.Role.ToString() : u.HealthcareStaff != null ? u.HealthcareStaff.Role.ToString() : u.Laboratory != null ? u.Laboratory.Role.ToString() : u.ImagingCenter != null ? u.ImagingCenter.Role.ToString() : u.SystemAdmin != null ? u.SystemAdmin.Role.ToString() : "")
                : query.OrderByDescending(u => u.Patient != null ? u.Patient.Role.ToString() : u.Doctor != null ? u.Doctor.Role.ToString() : u.HealthcareStaff != null ? u.HealthcareStaff.Role.ToString() : u.Laboratory != null ? u.Laboratory.Role.ToString() : u.ImagingCenter != null ? u.ImagingCenter.Role.ToString() : u.SystemAdmin != null ? u.SystemAdmin.Role.ToString() : ""),
            ListUsersSortBy.CreatedAt => sortDirection == SortDirection.Asc
                ? query.OrderBy(u => u.Patient != null ? u.Patient.CreatedAt : u.Doctor != null ? u.Doctor.CreatedAt : u.HealthcareStaff != null ? u.HealthcareStaff.CreatedAt : u.Laboratory != null ? u.Laboratory.CreatedAt : u.ImagingCenter != null ? u.ImagingCenter.CreatedAt : u.SystemAdmin != null ? u.SystemAdmin.CreatedAt : DateTime.MinValue)
                : query.OrderByDescending(u => u.Patient != null ? u.Patient.CreatedAt : u.Doctor != null ? u.Doctor.CreatedAt : u.HealthcareStaff != null ? u.HealthcareStaff.CreatedAt : u.Laboratory != null ? u.Laboratory.CreatedAt : u.ImagingCenter != null ? u.ImagingCenter.CreatedAt : u.SystemAdmin != null ? u.SystemAdmin.CreatedAt : DateTime.MinValue),
            ListUsersSortBy.NationalId => sortDirection == SortDirection.Asc
                ? query.OrderBy(u => u.Patient != null ? u.Patient.NationalId : u.Doctor != null ? u.Doctor.NationalId : u.HealthcareStaff != null ? u.HealthcareStaff.NationalId : u.Laboratory != null ? u.Laboratory.NationalId : u.ImagingCenter != null ? u.ImagingCenter.NationalId : u.SystemAdmin != null ? u.SystemAdmin.NationalId : "")
                : query.OrderByDescending(u => u.Patient != null ? u.Patient.NationalId : u.Doctor != null ? u.Doctor.NationalId : u.HealthcareStaff != null ? u.HealthcareStaff.NationalId : u.Laboratory != null ? u.Laboratory.NationalId : u.ImagingCenter != null ? u.ImagingCenter.NationalId : u.SystemAdmin != null ? u.SystemAdmin.NationalId : ""),
            _ => query.OrderBy(u => u.Id)
        };
        return sortDirection == SortDirection.Asc ? orderedQuery.ThenBy(u => u.Id) : orderedQuery.ThenByDescending(u => u.Id);
    }

    private static User MapToDomainUser(ApplicationUser user)
    {
        return (User?)user.Patient
            ?? (User?)user.Doctor
            ?? (User?)user.HealthcareStaff
            ?? (User?)user.Laboratory
            ?? (User?)user.ImagingCenter
            ?? (User?)user.SystemAdmin
            ?? throw new InvalidOperationException($"User {user.Id} has no associated domain entity.");
    }
}
