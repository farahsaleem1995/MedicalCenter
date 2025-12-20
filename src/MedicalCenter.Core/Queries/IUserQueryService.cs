using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.Core.Queries;

/// <summary>
/// Query service for retrieving all users entities (including practitioners and patients).
/// All users are retrieved, including deactivated users.
/// </summary>
public interface IUserQueryService
{
    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists users with pagination and optional filtering.
    /// </summary>
    Task<PaginatedList<User>> ListUsersPaginatedAsync(
        int pageNumber,
        int pageSize,
        UserRole? role = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
}

