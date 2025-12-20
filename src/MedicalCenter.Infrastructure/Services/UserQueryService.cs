using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Core.Queries;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Infrastructure.Data;
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
        return await QueryUsers(null, null)
            .Where(u => u.Id == id)
            .Select(u => MapToDomainUser(u))
            .FirstOrDefaultAsync(cancellationToken);
    } 

    public async Task<PaginatedList<User>> ListUsersPaginatedAsync(
        PaginationQuery<ListUsersQuery> query,
        CancellationToken cancellationToken = default)
    {
        return await QueryUsers(query.Criteria?.Role, query.Criteria?.IsActive)
            .Select(u => MapToDomainUser(u))
            .ToPaginatedListAsync(query.PageNumber, query.PageSize, cancellationToken);
    }

    private IQueryable<ApplicationUser> QueryUsers(UserRole? role, bool? isActive)
    {
        IQueryable<ApplicationUser> query = userManager.Users
            .IgnoreQueryFilters()
            .IncludeDomainEntities()
            .IncludeRoles();

        query = role.HasValue? query.WhereInRole(role.Value): query;

        query = isActive.HasValue 
            ? (isActive.Value ? query.WhereActive() : query.WhereInactive()) 
            : query;

        return query;
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
