using MedicalCenter.Core.Common;
using MedicalCenter.Core.Entities;
using MedicalCenter.Core.Enums;

namespace MedicalCenter.Core.Services;

/// <summary>
/// Query service for retrieving user entities (non-aggregate entities).
/// Used for read operations on provider entities (Doctor, HealthcareEntity, etc.).
/// </summary>
public interface IUserQueryService
{
    /// <summary>
    /// Gets a user by ID, returning the appropriate domain entity type.
    /// </summary>
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email, returning the appropriate domain entity type.
    /// </summary>
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists users with optional filtering by role and active status.
    /// </summary>
    Task<IReadOnlyList<User>> ListUsersAsync(
        UserRole? role = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists users with pagination and optional filtering by role and active status.
    /// </summary>
    Task<PaginatedList<User>> ListUsersPaginatedAsync(
        int pageNumber,
        int pageSize,
        UserRole? role = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a doctor by ID.
    /// </summary>
    Task<Doctor?> GetDoctorByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a healthcare entity by ID.
    /// </summary>
    Task<HealthcareEntity?> GetHealthcareEntityByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a laboratory by ID.
    /// </summary>
    Task<Laboratory?> GetLaboratoryByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an imaging center by ID.
    /// </summary>
    Task<ImagingCenter?> GetImagingCenterByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Admin query methods (ignore query filters to include deactivated users)

    /// <summary>
    /// Gets a user by ID, ignoring query filters (admin only).
    /// </summary>
    Task<User?> GetUserByIdAdminAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists users with optional filtering, ignoring query filters (admin only).
    /// </summary>
    Task<IReadOnlyList<User>> ListUsersAdminAsync(
        UserRole? role = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists users with pagination and optional filtering, ignoring query filters (admin only).
    /// </summary>
    Task<PaginatedList<User>> ListUsersAdminPaginatedAsync(
        int pageNumber,
        int pageSize,
        UserRole? role = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a doctor by ID, ignoring query filters (admin only).
    /// </summary>
    Task<Doctor?> GetDoctorByIdAdminAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a healthcare entity by ID, ignoring query filters (admin only).
    /// </summary>
    Task<HealthcareEntity?> GetHealthcareEntityByIdAdminAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a laboratory by ID, ignoring query filters (admin only).
    /// </summary>
    Task<Laboratory?> GetLaboratoryByIdAdminAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an imaging center by ID, ignoring query filters (admin only).
    /// </summary>
    Task<ImagingCenter?> GetImagingCenterByIdAdminAsync(Guid id, CancellationToken cancellationToken = default);
}

