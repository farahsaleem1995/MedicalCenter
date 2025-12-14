using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Common;
using MedicalCenter.Core.Entities;
using MedicalCenter.Core.Enums;

namespace MedicalCenter.Core.Services;

/// <summary>
/// Service for user identity operations (registration, password management, etc.).
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Creates a new Identity user (ApplicationUser) with the specified email and password.
    /// This is a generic method that creates the base Identity user.
    /// </summary>
    Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a doctor user with Identity and domain entity.
    /// </summary>
    Task<Result<Guid>> CreateDoctorAsync(
        string fullName,
        string email,
        string password,
        string licenseNumber,
        string specialty,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a healthcare entity user with Identity and domain entity.
    /// </summary>
    Task<Result<Guid>> CreateHealthcareEntityAsync(
        string fullName,
        string email,
        string password,
        string organizationName,
        string department,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a laboratory user with Identity and domain entity.
    /// </summary>
    Task<Result<Guid>> CreateLaboratoryAsync(
        string fullName,
        string email,
        string password,
        string labName,
        string licenseNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an imaging center user with Identity and domain entity.
    /// </summary>
    Task<Result<Guid>> CreateImagingCenterAsync(
        string fullName,
        string email,
        string password,
        string centerName,
        string licenseNumber,
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
    /// Admin can change a user's password without providing the current password.
    /// </summary>
    Task<Result> AdminChangePasswordAsync(
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

