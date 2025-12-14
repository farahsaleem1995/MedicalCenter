using MedicalCenter.Core.Common;
using MedicalCenter.Core.Entities;
using MedicalCenter.Core.Enums;

namespace MedicalCenter.Core.Services;

/// <summary>
/// Query service for retrieving all users entities (including providers and patients).
/// All users are retrieved, including deactivated users.
/// </summary>
public interface IUserQueryService
{
    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    Task<User?> GetUserByIdAdminAsync(Guid id, CancellationToken cancellationToken = default);

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

