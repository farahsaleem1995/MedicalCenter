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
    /// <param name="email">User email address</param>
    /// <param name="password">User password</param>
    /// <param name="role">User role</param>
    /// <param name="requireEmailConfirmation">If true, user starts with EmailConfirmed = false (for Patient role)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User ID if successful</returns>
    Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        UserRole role,
        bool requireEmailConfirmation = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user ID by email.
    /// </summary>
    Task<Guid?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

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
    /// Validates user credentials.
    /// </summary>
    Task<Result<Guid>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user's email is unconfirmed.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists and email is unconfirmed, false otherwise</returns>
    Task<bool> IsUserUnconfirmedAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a 6-digit email confirmation OTP code for a user.
    /// Uses Identity's stateless token generation internally.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>6-digit OTP code if successful</returns>
    Task<Result<string>> GenerateEmailConfirmationCodeAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms a user's email using the provided 6-digit OTP code.
    /// Uses Identity's stateless token verification internally.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="code">6-digit OTP code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> ConfirmEmailAsync(
        Guid userId,
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a 6-digit password reset OTP code for a user.
    /// Uses Identity's stateless token generation internally.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>6-digit OTP code if successful</returns>
    Task<Result<string>> GeneratePasswordResetCodeAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets a user's password using the provided 6-digit OTP code and new password.
    /// Uses Identity's stateless token verification internally.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="code">6-digit OTP code</param>
    /// <param name="newPassword">New password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> ResetPasswordAsync(
        Guid userId,
        string code,
        string newPassword,
        CancellationToken cancellationToken = default);
}

