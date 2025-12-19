using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.Core.Services;

/// <summary>
/// Service for user identity operations (registration, password management, etc.).
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Creates a new Identity user (ApplicationUser) with the specified email and password.
    /// This is a generic method that creates the base Identity user only.
    /// Domain entity creation should be handled by the calling endpoint.
    /// </summary>
    Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user's password without requiring the current password.
    /// Typically used by administrators to reset user passwords.
    /// </summary>
    Task<Result> UpdatePasswordAsync(
        Guid userId,
        string newPassword,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates user credentials.
    /// </summary>
    Task<Result<User>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a refresh token for a user.
    /// </summary>
    Task<Result> SaveRefreshTokenAsync(
        string token,
        Guid userId,
        DateTime expiryDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and retrieves user ID from a refresh token.
    /// </summary>
    Task<Result<Guid>> ValidateRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a refresh token (marks it as invalid).
    /// </summary>
    Task<Result> RevokeRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all refresh tokens for a user.
    /// </summary>
    Task<Result> InvalidateUserRefreshTokensAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

