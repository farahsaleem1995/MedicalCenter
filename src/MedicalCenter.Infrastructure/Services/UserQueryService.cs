using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Common;
using MedicalCenter.Core.Entities;
using MedicalCenter.Core.Enums;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of IUserQueryService for querying user entities.
/// Queries start from domain entities for database-level filtering.
/// </summary>
public class UserQueryService(
    MedicalCenterDbContext context,
    UserManager<ApplicationUser> userManager)
    : IUserQueryService
{
    private readonly UserQueryBuilder _queryBuilder = new(context, userManager);

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _queryBuilder.FindByIdAsync(id, cancellationToken);
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _queryBuilder.FindByEmailAsync(email, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> ListUsersAsync(
        UserRole? role = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        if (role.HasValue)
        {
            return await _queryBuilder.QueryByRoleAsync(role.Value, isActive, cancellationToken);
        }

        var allUsers = new List<User>();
        foreach (var r in Enum.GetValues<UserRole>())
        {
            var users = await _queryBuilder.QueryByRoleAsync(r, isActive, cancellationToken);
            allUsers.AddRange(users);
        }

        return allUsers;
    }

    public async Task<PaginatedList<User>> ListUsersPaginatedAsync(
        int pageNumber,
        int pageSize,
        UserRole? role = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        return await _queryBuilder.QueryByRolePaginatedAsync(role, isActive, pageNumber, pageSize, cancellationToken);
    }

    public async Task<Doctor?> GetDoctorByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Doctors.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<HealthcareEntity?> GetHealthcareEntityByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.HealthcareEntities.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<Laboratory?> GetLaboratoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Laboratories.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<ImagingCenter?> GetImagingCenterByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ImagingCenters.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    // Admin query methods (ignore query filters)

    public async Task<User?> GetUserByIdAdminAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _queryBuilder.IgnoreQueryFilters().FindByIdAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> ListUsersAdminAsync(
        UserRole? role = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        if (role.HasValue)
        {
            return await _queryBuilder.IgnoreQueryFilters().QueryByRoleAsync(role.Value, isActive, cancellationToken);
        }

        var allUsers = new List<User>();
        foreach (var r in Enum.GetValues<UserRole>())
        {
            var users = await _queryBuilder.IgnoreQueryFilters().QueryByRoleAsync(r, isActive, cancellationToken);
            allUsers.AddRange(users);
        }

        return allUsers;
    }

    public async Task<PaginatedList<User>> ListUsersAdminPaginatedAsync(
        int pageNumber,
        int pageSize,
        UserRole? role = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        return await _queryBuilder.IgnoreQueryFilters().QueryByRolePaginatedAsync(role, isActive, pageNumber, pageSize, cancellationToken);
    }

    public async Task<Doctor?> GetDoctorByIdAdminAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Doctors.IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<HealthcareEntity?> GetHealthcareEntityByIdAdminAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.HealthcareEntities.IgnoreQueryFilters()
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<Laboratory?> GetLaboratoryByIdAdminAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Laboratories.IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<ImagingCenter?> GetImagingCenterByIdAdminAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ImagingCenters.IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }
}

/// <summary>
/// Builder for constructing user queries with fluent API.
/// Encapsulates query logic and reduces complexity in UserQueryService.
/// </summary>
internal class UserQueryBuilder
{
    private readonly MedicalCenterDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private bool _ignoreQueryFilters;

    public UserQueryBuilder(MedicalCenterDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public UserQueryBuilder IgnoreQueryFilters()
    {
        _ignoreQueryFilters = true;
        return this;
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await TryFindInDomainEntitiesAsync(id, cancellationToken)
            ?? await TryFindSystemAdminAsync(id, cancellationToken);
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await TryFindInDomainEntitiesByEmailAsync(email, cancellationToken)
            ?? await TryFindSystemAdminByEmailAsync(email, cancellationToken);
    }

    public async Task<List<User>> QueryByRoleAsync(
        UserRole role,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        return role switch
        {
            UserRole.Doctor => await QueryDoctorsAsync(isActive, cancellationToken),
            UserRole.HealthcareStaff => await QueryHealthcareEntitiesAsync(isActive, cancellationToken),
            UserRole.LabUser => await QueryLaboratoriesAsync(isActive, cancellationToken),
            UserRole.ImagingUser => await QueryImagingCentersAsync(isActive, cancellationToken),
            UserRole.Patient => await QueryPatientsAsync(isActive, cancellationToken),
            UserRole.SystemAdmin => await QuerySystemAdminsAsync(isActive, cancellationToken),
            _ => new List<User>()
        };
    }

    public async Task<PaginatedList<User>> QueryByRolePaginatedAsync(
        UserRole? role,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (role.HasValue)
        {
            return await QueryByRolePaginatedAsync(role.Value, isActive, pageNumber, pageSize, cancellationToken);
        }

        // When no role specified, query all roles and combine
        var allUsers = new List<User>();
        foreach (var r in Enum.GetValues<UserRole>())
        {
            var users = await QueryByRoleAsync(r, isActive, cancellationToken);
            allUsers.AddRange(users);
        }

        // Apply pagination to combined results
        var totalCount = allUsers.Count;
        var skip = (pageNumber - 1) * pageSize;
        var paginatedItems = allUsers.Skip(skip).Take(pageSize).ToList();

        return new PaginatedList<User>(paginatedItems, pageNumber, pageSize, totalCount);
    }

    private async Task<PaginatedList<User>> QueryByRolePaginatedAsync(
        UserRole role,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return role switch
        {
            UserRole.Doctor => await QueryDoctorsPaginatedAsync(isActive, pageNumber, pageSize, cancellationToken),
            UserRole.HealthcareStaff => await QueryHealthcareEntitiesPaginatedAsync(isActive, pageNumber, pageSize, cancellationToken),
            UserRole.LabUser => await QueryLaboratoriesPaginatedAsync(isActive, pageNumber, pageSize, cancellationToken),
            UserRole.ImagingUser => await QueryImagingCentersPaginatedAsync(isActive, pageNumber, pageSize, cancellationToken),
            UserRole.Patient => await QueryPatientsPaginatedAsync(isActive, pageNumber, pageSize, cancellationToken),
            UserRole.SystemAdmin => await QuerySystemAdminsPaginatedAsync(isActive, pageNumber, pageSize, cancellationToken),
            _ => PaginatedList<User>.Empty(pageNumber, pageSize)
        };
    }

    private async Task<User?> TryFindInDomainEntitiesAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery();
        
        return await query.Doctors.FirstOrDefaultAsync(d => d.Id == id, cancellationToken) as User
            ?? await query.HealthcareEntities.FirstOrDefaultAsync(h => h.Id == id, cancellationToken) as User
            ?? await query.Laboratories.FirstOrDefaultAsync(l => l.Id == id, cancellationToken) as User
            ?? await query.ImagingCenters.FirstOrDefaultAsync(i => i.Id == id, cancellationToken) as User
            ?? await query.Patients.FirstOrDefaultAsync(p => p.Id == id, cancellationToken) as User;
    }

    private async Task<User?> TryFindInDomainEntitiesByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery();
        
        return await query.Doctors.FirstOrDefaultAsync(d => d.Email == email, cancellationToken) as User
            ?? await query.HealthcareEntities.FirstOrDefaultAsync(h => h.Email == email, cancellationToken) as User
            ?? await query.Laboratories.FirstOrDefaultAsync(l => l.Email == email, cancellationToken) as User
            ?? await query.ImagingCenters.FirstOrDefaultAsync(i => i.Email == email, cancellationToken) as User
            ?? await query.Patients.FirstOrDefaultAsync(p => p.Email == email, cancellationToken) as User;
    }

    private async Task<User?> TryFindSystemAdminAsync(Guid id, CancellationToken cancellationToken)
    {
        var identityUser = await _userManager.FindByIdAsync(id.ToString());
        if (identityUser == null) return null;

        var roles = await _userManager.GetRolesAsync(identityUser);
        return roles.Count > 0 && Enum.TryParse<UserRole>(roles[0], out var role) && role == UserRole.SystemAdmin
            ? new IdentityUserWrapper(identityUser, role)
            : null;
    }

    private async Task<User?> TryFindSystemAdminByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var identityUser = await _userManager.FindByEmailAsync(email);
        if (identityUser == null) return null;

        var roles = await _userManager.GetRolesAsync(identityUser);
        return roles.Count > 0 && Enum.TryParse<UserRole>(roles[0], out var role) && role == UserRole.SystemAdmin
            ? new IdentityUserWrapper(identityUser, role)
            : null;
    }

    private async Task<List<User>> QueryDoctorsAsync(bool? isActive, CancellationToken cancellationToken)
    {
        var query = ApplyIsActiveFilter(GetBaseQuery().Doctors, isActive);
        return (await query.ToListAsync(cancellationToken)).Cast<User>().ToList();
    }

    private async Task<List<User>> QueryHealthcareEntitiesAsync(bool? isActive, CancellationToken cancellationToken)
    {
        var query = ApplyIsActiveFilter(GetBaseQuery().HealthcareEntities, isActive);
        return (await query.ToListAsync(cancellationToken)).Cast<User>().ToList();
    }

    private async Task<List<User>> QueryLaboratoriesAsync(bool? isActive, CancellationToken cancellationToken)
    {
        var query = ApplyIsActiveFilter(GetBaseQuery().Laboratories, isActive);
        return (await query.ToListAsync(cancellationToken)).Cast<User>().ToList();
    }

    private async Task<List<User>> QueryImagingCentersAsync(bool? isActive, CancellationToken cancellationToken)
    {
        var query = ApplyIsActiveFilter(GetBaseQuery().ImagingCenters, isActive);
        return (await query.ToListAsync(cancellationToken)).Cast<User>().ToList();
    }

    private async Task<List<User>> QueryPatientsAsync(bool? isActive, CancellationToken cancellationToken)
    {
        var query = ApplyIsActiveFilter(GetBaseQuery().Patients, isActive);
        return (await query.ToListAsync(cancellationToken)).Cast<User>().ToList();
    }

    private async Task<List<User>> QuerySystemAdminsAsync(bool? isActive, CancellationToken cancellationToken)
    {
        var roleName = UserRole.SystemAdmin.ToString();
        var identityUsers = await _userManager.GetUsersInRoleAsync(roleName);
        
        return identityUsers
            .Select(identityUser => new IdentityUserWrapper(identityUser, UserRole.SystemAdmin))
            .Where(wrapper => !isActive.HasValue || wrapper.IsActive == isActive.Value)
            .Cast<User>()
            .ToList();
    }

    private async Task<PaginatedList<User>> QueryDoctorsPaginatedAsync(bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = ApplyIsActiveFilter(GetBaseQuery().Doctors, isActive);
        return await QueryPaginatedAsync(query, pageNumber, pageSize, cancellationToken);
    }

    private async Task<PaginatedList<User>> QueryHealthcareEntitiesPaginatedAsync(bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = ApplyIsActiveFilter(GetBaseQuery().HealthcareEntities, isActive);
        return await QueryPaginatedAsync(query, pageNumber, pageSize, cancellationToken);
    }

    private async Task<PaginatedList<User>> QueryLaboratoriesPaginatedAsync(bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = ApplyIsActiveFilter(GetBaseQuery().Laboratories, isActive);
        return await QueryPaginatedAsync(query, pageNumber, pageSize, cancellationToken);
    }

    private async Task<PaginatedList<User>> QueryImagingCentersPaginatedAsync(bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = ApplyIsActiveFilter(GetBaseQuery().ImagingCenters, isActive);
        return await QueryPaginatedAsync(query, pageNumber, pageSize, cancellationToken);
    }

    private async Task<PaginatedList<User>> QueryPatientsPaginatedAsync(bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = ApplyIsActiveFilter(GetBaseQuery().Patients, isActive);
        return await QueryPaginatedAsync(query, pageNumber, pageSize, cancellationToken);
    }

    private async Task<PaginatedList<User>> QuerySystemAdminsPaginatedAsync(bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var roleName = UserRole.SystemAdmin.ToString();
        var identityUsers = await _userManager.GetUsersInRoleAsync(roleName);
        
        var allAdmins = identityUsers
            .Select(identityUser => new IdentityUserWrapper(identityUser, UserRole.SystemAdmin))
            .Where(wrapper => !isActive.HasValue || wrapper.IsActive == isActive.Value)
            .Cast<User>()
            .ToList();

        var totalCount = allAdmins.Count;
        var skip = (pageNumber - 1) * pageSize;
        var paginatedItems = allAdmins.Skip(skip).Take(pageSize).ToList();

        return new PaginatedList<User>(paginatedItems, pageNumber, pageSize, totalCount);
    }

    private static async Task<PaginatedList<User>> QueryPaginatedAsync<T>(IQueryable<T> query, int pageNumber, int pageSize, CancellationToken cancellationToken) where T : User
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var skip = (pageNumber - 1) * pageSize;
        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<User>(items.Cast<User>().ToList(), pageNumber, pageSize, totalCount);
    }

    private static IQueryable<T> ApplyIsActiveFilter<T>(IQueryable<T> query, bool? isActive) where T : User
    {
        return isActive.HasValue
            ? query.Where(u => u.IsActive == isActive.Value)
            : query;
    }

    private (IQueryable<Doctor> Doctors, IQueryable<HealthcareEntity> HealthcareEntities, 
             IQueryable<Laboratory> Laboratories, IQueryable<ImagingCenter> ImagingCenters, 
             IQueryable<Patient> Patients) GetBaseQuery()
    {
        if (_ignoreQueryFilters)
        {
            return (
                _context.Doctors.IgnoreQueryFilters(),
                _context.HealthcareEntities.IgnoreQueryFilters(),
                _context.Laboratories.IgnoreQueryFilters(),
                _context.ImagingCenters.IgnoreQueryFilters(),
                _context.Patients.IgnoreQueryFilters()
            );
        }

        return (
            _context.Doctors,
            _context.HealthcareEntities,
            _context.Laboratories,
            _context.ImagingCenters,
            _context.Patients
        );
    }

    /// <summary>
    /// Simple wrapper to convert Identity ApplicationUser to domain User for SystemAdmin.
    /// </summary>
    private class IdentityUserWrapper : User
    {
        public IdentityUserWrapper(ApplicationUser identityUser, UserRole role)
            : base(identityUser.UserName ?? identityUser.Email ?? string.Empty, identityUser.Email ?? string.Empty, role)
        {
            Id = identityUser.Id;
            IsActive = !identityUser.LockoutEnabled || identityUser.LockoutEnd == null || identityUser.LockoutEnd <= DateTimeOffset.UtcNow;
        }
    }
}
